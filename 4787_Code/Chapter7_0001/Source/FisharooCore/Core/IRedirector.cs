using System;
using StructureMap;

namespace Fisharoo.FisharooCore.Core
{
    [PluginFamily("Default")]
    public interface IRedirector
    {
        void GoToPhotosMyPhotos();
        void GoToHomePage();
        void GoToErrorPage();
        void GoToProfilesProfile();
        void GoToProfilesDefault();
        void GoToProfilesManageProfile();
        void GoToAccountLoginPage(string FriendInvitationKey);
        void GoToAccountLoginPage();
        void GoToAccountRegisterPage(string FriendInvitationKey);
        void GoToAccountRegisterPage();
        void GoToAccountEditAccountPage();
        void GoToAccountRecoverPasswordPage();
        void GoToAccountAccessDenied();
        void GoToSearch(string SearchText);
        void GoToFriendsInviteFriends(Int32 AccountIdToInvite);
        void GoToProfilesStatusUpdates();
        void GoToProfilesStatusUpdates(Int32 AccountID);
        void GoToMailNewMessage(Int32 MessageID);
        void GoToPhotosAddPhotos(Int64 AlbumID);
        void GoToPhotos();
        void GoToPhotosViewAlbum(Int64 AlbumID);
        void GoToPhotosEditPhotos(Int64 AlbumID);
        void GoToPhotosEditAlbum(Int64 AlbumID);
    }
}