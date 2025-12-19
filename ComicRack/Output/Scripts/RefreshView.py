<<<<<<< HEAD
import clr_bridge
from clr_bridge import ComicRack
=======
import clr

clr.AddReferenceByPartialName('ComicRack.Engine')
clr.AddReferenceByPartialName('cYo.Common.Windows')

>>>>>>> upstream/master
from cYo.Common.Windows.Forms import FormUtility
from cYo.Projects.ComicRack.Engine import IRefreshDisplay
	
#@Name Refresh View
#@Key RefreshView
#@Image Refresh.png
#@Hook Books
def RefreshDisplay(books): 
<<<<<<< HEAD
    FormUtility.FindActiveService[IRefreshDisplay]().RefreshDisplay()
=======
    FormUtility.FindActiveService[IRefreshDisplay]().RefreshDisplay()
>>>>>>> upstream/master
