using System;
using System.IO;
using Xunit;
using ComicRack.Plugins;

namespace ComicRack.Plugins.Tests
{
    public class RefreshViewTests
    {
        [Fact]
        public void LoadRefreshViewScript_ShouldThrowFileNotFound()
        {
            // Arrange
            var manager = PythonRuntimeManager.Instance;
            manager.Initialize();
            var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "ComicRack", "bin", "Debug", "net9.0-windows", "Scripts", "RefreshView.py");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => manager.LoadScript(scriptPath));
        }
    }
}
