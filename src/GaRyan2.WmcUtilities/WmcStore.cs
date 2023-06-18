using GaRyan2.Utilities;
using Microsoft.MediaCenter.Store;
using System;
using System.Security.Cryptography;
using System.Text;

namespace GaRyan2.WmcUtilities
{
    public static partial class WmcStore
    {
        private static ObjectStore _objectStore;

        public static bool StoreExpired;

        public static ObjectStore WmcObjectStore
        {
            get
            {
                if (_objectStore != null && !StoreExpired) return _objectStore;
                var sha256Man = new SHA256Managed();
                ObjectStore.FriendlyName = @"Anonymous!User";
                ObjectStore.DisplayName = Convert.ToBase64String(sha256Man.ComputeHash(Encoding.Unicode.GetBytes(ObjectStore.GetClientId(true))));
                _objectStore = ObjectStore.AddObjectStoreReference();
                StoreExpired = false;

                _objectStore.StoreExpired += WmcObjectStore_StoreExpired;
                return _objectStore;
            }
        }

        private static void WmcObjectStore_StoreExpired(object sender, StoredObjectEventArgs e)
        {
            Logger.WriteError("A database recovery has been detected. Attempting to open new database.");
            Close();
            StoreExpired = true;
            _objectStore.StoreExpired -= WmcObjectStore_StoreExpired;
            if (WmcObjectStore != null)
            {
                Logger.WriteInformation("Successfully opened new store.");
            }
        }

        /// <summary>
        /// Closes the WMC ObjectStore with the option to dispose the ObjectStore
        /// </summary>
        /// <param name="dispose">true = dispose ObjectStore</param>
        public static void Close(bool dispose = false)
        {
            if (_objectStore != null)
            {
                _objectStore.StoreExpired -= WmcObjectStore_StoreExpired;
                ObjectStore.ReleaseObjectStoreReference();
            }
            _mergedLineup = null;

            try
            {
                if (dispose)
                {
                    _objectStore?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Exception thrown while trying to dispose of ObjectStore. {ex}");
            }
            _objectStore = null;
            GC.WaitForPendingFinalizers();
        }
    }
}