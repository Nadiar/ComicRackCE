using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Security;
using CoreWCF.Channels;
using CoreWCF.Description;
using System.Security.Authentication;
using System.IdentityModel.Tokens;
using cYo.Common;
using cYo.Common.Collections;
using cYo.Common.ComponentModel;
using cYo.Common.Drawing;
using cYo.Common.Net;
using cYo.Common.Threading;
using cYo.Projects.ComicRack.Engine.Database;
using cYo.Projects.ComicRack.Engine.IO.Cache;
using cYo.Projects.ComicRack.Engine.IO.Provider;
using cYo.Projects.ComicRack.Engine.Properties;

namespace cYo.Projects.ComicRack.Engine.IO.Network
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Prefix)]
	public class ComicLibraryServer : IRemoteComicLibrary, IRemoteServerInfo, IDisposable
	{
		private class PasswordValidator : CoreWCF.IdentityModel.Selectors.UserNamePasswordValidator
		{
			private readonly string password;

			public PasswordValidator(string password)
			{
				this.password = password;
			}

			public override ValueTask ValidateAsync(string userName, string password)
			{
				if (!string.IsNullOrEmpty(this.password) && this.password != password)
				{
					throw new SecurityTokenException("Validation Failed!");
				}
                return ValueTask.CompletedTask;
			}
		}

		public const string InfoPoint = "Info";
		public const string LibraryPoint = "Library";
		public static string ExternalServerAddress;
		private static X509Certificate2 certificate;
        
        // Static Host for CoreWCF
        private static WebApplication _host;
        private static readonly object _hostLock = new object();

		private readonly Func<ComicLibrary> getComicLibrary;
		private readonly Cache<Guid, IImageProvider> providerCache = new Cache<Guid, IImageProvider>(EngineConfiguration.Default.ServerProviderCacheSize);

		public static X509Certificate2 Certificate
		{
			get
			{
				if (certificate == null)
				{
					// CoreWCF / .NET 9 Migration: X509Certificate2 constructor is obsolete.
                    // Assuming Resources.Certificate2 is a PKCS#12 (PFX) blob given it has a password argument.
					certificate = X509CertificateLoader.LoadPkcs12(Resources.Certificate2, string.Empty);
                }
				return certificate;
			}
		}

		public string Id { get; private set; }
		public ComicLibraryServerConfig Config { get; private set; }
		public IBroadcast<BroadcastData> Broadcaster { get; private set; }
		public bool PingEnabled { get; set; }
		public IPagePool PagePool { get; set; }
		public IThumbnailPool ThumbPool { get; set; }
		public ComicLibrary ComicLibrary => getComicLibrary();
		public ServerStatistics Statistics { get; private set; }
public bool IsRunning => _host != null && _host.Lifetime.ApplicationStopping.IsCancellationRequested == false; 
        // Note: Tracking 'IsRunning' for specific instance in shared host is complex, simplified for now.
		
		string IRemoteServerInfo.Id
		{
			get
			{
				CheckPrivateNetwork();
				AddStats(ServerStatistics.StatisticType.InfoRequest);
				return Id;
			}
		}

		string IRemoteServerInfo.Name => Config.Name;
		string IRemoteServerInfo.Description => Config.Description;
		ServerOptions IRemoteServerInfo.Options => Config.Options;

		public bool IsValid
		{
			get
			{
				try
				{
					CheckPrivateNetwork();
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		public ComicLibraryServer(ComicLibraryServerConfig config, Func<ComicLibrary> getComicLibrary, IPagePool pagePool, IThumbnailPool thumbPool, IBroadcast<BroadcastData> broadcaster)
		{
			Id = Guid.NewGuid().ToString();
			Config = CloneUtility.Clone(config);
			Statistics = new ServerStatistics();
			this.getComicLibrary = getComicLibrary;
			PagePool = pagePool;
			ThumbPool = thumbPool;
			Broadcaster = broadcaster;
			PingEnabled = true;
			providerCache.ItemRemoved += providerCache_ItemRemoved;
		}

		public void Dispose()
		{
			Stop();
			providerCache.Dispose();
		}

		byte[] IRemoteComicLibrary.GetLibraryData()
		{
			CheckPrivateNetwork();
			try
			{
				byte[] array = GetSharedComicLibrary().ToByteArray();
				AddStats(ServerStatistics.StatisticType.LibraryRequest, array.Length);
				return array;
			}
			catch (Exception)
			{
				return null;
			}
		}

		int IRemoteComicLibrary.GetImageCount(Guid comicGuid)
		{
			ComicBook comicBook = ComicLibrary.Books[comicGuid];
			try
			{
				if (comicBook.PageCount > 0)
				{
					return comicBook.PageCount;
				}
				using (IItemLock<IImageProvider> itemLock = providerCache.LockItem(comicGuid, CreateProvider))
				{
					comicBook.PageCount = itemLock.Item.Count;
					return comicBook.PageCount;
				}
			}
			catch (Exception)
			{
				return 0;
			}
		}

		byte[] IRemoteComicLibrary.GetImage(Guid comicGuid, int index)
		{
			CheckPrivateNetwork();
			ComicBook comicBook = ComicLibrary.Books[comicGuid];
			try
			{
				index = comicBook.TranslateImageIndexToPage(index);
				using (IItemLock<IImageProvider> itemLock = providerCache.LockItem(comicGuid, CreateProvider))
				{
					using (IItemLock<PageImage> itemLock2 = PagePool.GetPage(comicBook.GetPageKey(index, BitmapAdjustment.Empty), itemLock.Item, onErrorThrowException: true))
					{
						int pageQuality = Config.PageQuality;
						byte[] array = ((pageQuality != 100) ? itemLock2.Item.Bitmap.ImageToJpegBytes(75 * pageQuality / 100) : ((byte[])itemLock2.Item.Data.Clone()));
						AddStats(ServerStatistics.StatisticType.PageRequest, array.Length);
						return array;
					}
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		byte[] IRemoteComicLibrary.GetThumbnailImage(Guid comicGuid, int index)
		{
			ComicBook comicBook = ComicLibrary.Books[comicGuid];
			try
			{
				index = comicBook.TranslateImageIndexToPage(index);
				using (IItemLock<IImageProvider> itemLock = providerCache.LockItem(comicGuid, CreateProvider))
				{
					using (IItemLock<ThumbnailImage> itemLock2 = ThumbPool.GetThumbnail(comicBook.GetThumbnailKey(index), itemLock.Item, onErrorThrowException: true))
					{
						int thumbnailQuality = Config.ThumbnailQuality;
						byte[] array = ((thumbnailQuality != 100) ? new ThumbnailImage(itemLock2.Item.Bitmap.ImageToJpegBytes(75 * thumbnailQuality / 100), itemLock2.Item.Size, itemLock2.Item.OriginalSize).ToBytes() : itemLock2.Item.ToBytes());
						AddStats(ServerStatistics.StatisticType.ThumbnailRequest, array.Length);
						return array;
					}
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		void IRemoteComicLibrary.UpdateComic(Guid comicGuid, string propertyName, object value)
		{
			if (Config.IsEditable)
			{
				try
				{
					ComicLibrary.Books[comicGuid]?.SetValue(propertyName, value);
				}
				catch (Exception)
				{
				}
			}
		}

		public string GetAnnouncementUri()
		{
			if (!Config.IsInternet)
			{
				return null;
			}
			return ServiceAddress.CompletePortAndPath(GetExternalServiceAddress(), (Config.ServicePort == ComicLibraryServerConfig.DefaultPublicServicePort) ? null : Config.ServicePort.ToString(), (Config.ServiceName == ComicLibraryServerConfig.DefaultServiceName) ? null : Config.ServiceName);
		}

		public void AnnounceServer()
		{
            // Announcement logic disabled for now or needs reimplementation without WCF client to self?
            return;
		}

		public void AnnouncedServerRefresh() {}
		public void AnnouncedServerRemove() {}

		public void CheckPrivateNetwork()
		{
			if (!Config.OnlyPrivateConnections || GetClientIp().IsPrivate())
			{
				return;
			}
			throw new AuthenticationException("Only clients in private network can connect");
		}

		public void AddStats(ServerStatistics.StatisticType type, int size = 0)
		{
            var ip = GetClientIp();
            if (ip != null)
			    Statistics.Add(ip.ToString(), type, size);
		}

		public void Stop()
		{
            lock (_hostLock)
            {
                if (_host != null)
                {
                    _host.StopAsync().Wait();
                    _host.DisposeAsync().AsTask().Wait();
                    _host = null;
                }
            }
		}

		private ComicLibrary GetSharedComicLibrary()
		{
			switch (Config.LibraryShareMode)
			{
			case LibraryShareMode.Selected:
			{
				ComicLibrary comicLibrary = new ComicLibrary
				{
					Name = ComicLibrary.Name,
					Id = ComicLibrary.Id
				};
				HashSet<ComicBook> hashSet = new HashSet<ComicBook>();
				IEnumerable<ShareableComicListItem> source = from scli in ComicLibrary.ComicLists.GetItems<ShareableComicListItem>()
					where Config.SharedItems.Contains(scli.Id)
					select scli;
				comicLibrary.ComicLists.AddRange(source.Select((ShareableComicListItem scli) => new ComicIdListItem(scli)));
				hashSet.AddRange(source.SelectMany((ShareableComicListItem scli) => scli.GetBooks()));
				comicLibrary.Books.AddRange(hashSet.Select((ComicBook cb) => new ComicBook(cb)));
				return comicLibrary;
			}
			case LibraryShareMode.All:
				return ComicLibrary.Attach(ComicLibrary);
			default:
				return new ComicLibrary();
			}
		}

		private void providerCache_ItemRemoved(object sender, CacheItemEventArgs<Guid, IImageProvider> e)
		{
			e.Item.Dispose();
		}

		private IImageProvider CreateProvider(Guid comicGuid)
		{
			ComicBook comicBook = ComicLibrary.Books[comicGuid];
			return comicBook.OpenProvider();
		}

		[CLSCompliant(false)]
		public static CoreWCF.Channels.Binding CreateServerChannel(bool secure)
        {
            var netTcpBinding = new CoreWCF.NetTcpBinding();
            netTcpBinding.Security.Mode = (secure ? CoreWCF.SecurityMode.Message : CoreWCF.SecurityMode.None);
            if (secure)
            {
                netTcpBinding.Security.Message.ClientCredentialType = CoreWCF.MessageCredentialType.UserName;
            }
            netTcpBinding.MaxReceivedMessageSize = 100000000L;
            netTcpBinding.ReaderQuotas.MaxArrayLength = 100000000;
            return netTcpBinding;
        }

        /// <summary>
        /// Creates a client-side WCF binding compatible with the CoreWCF server configuration.
        /// WHAT: Returns a System.ServiceModel.Channels.Binding instance.
        /// WHY: Required because the client code (System.ServiceModel) cannot use CoreWCF types directly, 
        /// and we need to match the specific security mode and quota settings defined on the server.
        /// </summary>
        public static System.ServiceModel.Channels.Binding CreateClientChannel(bool secure)
        {
            var netTcpBinding = new System.ServiceModel.NetTcpBinding();
            netTcpBinding.Security.Mode = (secure ? System.ServiceModel.SecurityMode.Message : System.ServiceModel.SecurityMode.None);
            if (secure)
            {
                netTcpBinding.Security.Message.ClientCredentialType = System.ServiceModel.MessageCredentialType.UserName;
            }
            netTcpBinding.MaxReceivedMessageSize = 100000000L;
            netTcpBinding.ReaderQuotas.MaxArrayLength = 100000000;
            return netTcpBinding;
        }

		public static string GetExternalServiceAddress()
		{
			string text = ExternalServerAddress ?? string.Empty;
			text = text.Trim();
			try
			{
				if (string.IsNullOrEmpty(text))
				{
					return ServiceAddress.GetWanAddress();
				}
				return text;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static IEnumerable<ComicLibraryServer> Start(IEnumerable<ComicLibraryServerConfig> servers, int port, Func<ComicLibrary> getComicLibrary, IPagePool pagePool, IThumbnailPool thumbPool, IBroadcast<BroadcastData> broadcaster)
		{
            lock (_hostLock)
            {
                if (_host != null) throw new InvalidOperationException("Host already running. Restarting not fully supported in this shim.");

                var activeServers = new List<ComicLibraryServer>();
                var builder = WebApplication.CreateBuilder();
                
                // Configure Kestrel to listen on NetTcp port
                builder.WebHost.UseNetTcp(port);

                builder.Services.AddServiceModelServices();
                builder.Services.AddServiceModelMetadata();
                
                // Single Authentication config for shared host?
                // Note: If multiple servers have different passwords, global auth logic needs to dispatch.
                // Assuming FIRST Valid Share determines the generic config for now.
                var firstConfig = servers.FirstOrDefault(c => c.IsValidShare);
                if (firstConfig != null) {
                    builder.Services.AddSingleton<CoreWCF.IdentityModel.Selectors.UserNamePasswordValidator>(new PasswordValidator(firstConfig.ProtectionPassword));
                }

                // Register Instance(s)
                foreach (ComicLibraryServerConfig item in servers.Where((ComicLibraryServerConfig c) => c.IsValidShare))
                {
                    // Supporting only ONE instance for now to avoid complexity
                    
                    item.ServicePort = port;
				    item.ServiceName = "Share"; // Force simple name or logic for dispatch?
                    
                    var serverInstance = new ComicLibraryServer(item, getComicLibrary, pagePool, thumbPool, broadcaster);
                    activeServers.Add(serverInstance);
                    
                    // Register AS SINGLETON. If multiple, only first works in current DI model without Named Services.
                    if (activeServers.Count == 1) 
                    {
                        builder.Services.AddSingleton<ComicLibraryServer>(serverInstance);
                    }
                }

                if (activeServers.Count == 0) return Enumerable.Empty<ComicLibraryServer>();

                _host = builder.Build();

                _host.UseServiceModel(serviceBuilder =>
                {
                    // Configure Service
                    var serviceConfig = activeServers.First().Config;
                    serviceBuilder.AddService<ComicLibraryServer>(serviceOptions => {
                         serviceOptions.BaseAddresses.Add(new Uri($"net.tcp://localhost:{port}/{serviceConfig.ServiceName}"));
                    })
                    .AddServiceEndpoint<ComicLibraryServer, IRemoteServerInfo>(CreateServerChannel(false), InfoPoint)
                    .AddServiceEndpoint<ComicLibraryServer, IRemoteComicLibrary>(CreateServerChannel(true), LibraryPoint);

                    // Configure Service Credentials via ConfigureServiceHostBase
                    serviceBuilder.ConfigureServiceHostBase<ComicLibraryServer>(host =>
                    {
                        var serviceHost = host as CoreWCF.ServiceHostBase;
                        if (serviceHost != null)
                        {
                            // WHAT: Access Credentials via the ServiceHostBase property.
                            // WHY: We cast to CoreWCF.ServiceHostBase because directly using the `ServiceCredentialsBehavior` type 
                            // was causing compilation ambiguity/resolution errors (`CS0234`).
                            // This approach allows us to configure UserName authentication and Certificates without explicit type references that were failing.
                            var serviceCredentials = serviceHost.Credentials;
                            
                            serviceCredentials.UserNameAuthentication.UserNamePasswordValidationMode = CoreWCF.Security.UserNamePasswordValidationMode.Custom;
                            serviceCredentials.UserNameAuthentication.CustomUserNamePasswordValidator = new PasswordValidator(serviceConfig.ProtectionPassword);
                            serviceCredentials.ServiceCertificate.Certificate = Certificate;
                            serviceCredentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
                        }
                    });
                });

                _host.StartAsync(); // Fire and forget (it blocks Thread if Run(), StartAsync is non-blocking)
                
                return activeServers;
            }
		}

		/// <summary>
		/// STUB: Placeholder for server discovery.
		/// WHAT: Returns an empty list of public servers.
		/// WHY: The original `GetPublicServers` method likely relied on WCF Discovery or proprietary UDP broadcasts 
		/// which need to be reimplemented for .NET 9 / CoreWCF. 
		/// Added this stub to resolve compilation errors (`CS0117`) in `OpenRemoteDialog.cs`.
		/// </summary>
		public static IEnumerable<ShareInformation> GetPublicServers(ServerOptions options, string password)
		{
            // TODO: Implement discovery mechanism compatible with CoreWCF / .NET 9
            // Previously likely used WCF Discovery or UDP broadcast.
			return Enumerable.Empty<ShareInformation>();
		}

		public static IPAddress GetClientIp()
		{
            // CoreWCF specific way to get Remote IP
			var context = OperationContext.Current;
            if (context == null) return null;
			var prop = context.IncomingMessageProperties;
			var remoteEndpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            if (remoteEndpoint != null && IPAddress.TryParse(remoteEndpoint.Address, out var address)) return address;
			return null;
		}
	}
}
