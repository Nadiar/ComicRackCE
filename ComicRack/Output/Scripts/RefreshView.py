import clr_bridge
from clr_bridge import ComicRack
from cYo.Common.Windows.Forms import FormUtility
from cYo.Projects.ComicRack.Engine import IRefreshDisplay
	
#@Name Refresh View
#@Key RefreshView
#@Image Refresh.png
#@Hook Books
def RefreshDisplay(books): 
    FormUtility.FindActiveService[IRefreshDisplay]().RefreshDisplay()
