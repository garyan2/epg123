using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Store;

namespace epg123
{
    public static class Store
    {
        private static bool inhibit = false;
        private static ObjectStore objectStore_;
        private static MergedLineup mergedLineup_;
        private static bool blocking_background_threads_ = false;

        public static ObjectStore objectStore
        {
            get
            {
                if (!inhibit && objectStore_ == null)
                {
                    if (!blocking_background_threads_)
                    {
                        ObjectStore.WaitForThenBlockBackgroundThreads(0x7ffffff);
                        blocking_background_threads_ = true;
                    }

                    SHA256Managed sha256Man = new SHA256Managed();
                    string clientId = ObjectStore.GetClientId(true);
                    string providerName = @"Anonymous!User";
                    string password = Convert.ToBase64String(sha256Man.ComputeHash(Encoding.Unicode.GetBytes(clientId)));
                    objectStore_ = ObjectStore.Open("", providerName, password, true);
                }
                return objectStore_;
            }
        }

        public static MergedLineup mergedLineup
        {
            get
            {
                if (!inhibit && objectStore != null && mergedLineup_ == null)
                {
                    using (MergedLineups mergedLineups = new MergedLineups(objectStore))
                    {
                        foreach (MergedLineup lineup in mergedLineups)
                        {
                            if (lineup.GetChannels().Length > 0)
                            {
                                mergedLineup_ = lineup;
                            }
                        }
                    }
                }
                return mergedLineup_;
            }
        }

        public static void Close(bool reopen = false)
        {
            inhibit = true;

            if (mergedLineup != null)
            {
                mergedLineup.FullMerge(false);
                mergedLineup.Update();
                mergedLineup_ = null;
            }

            if (objectStore != null)
            {
                objectStore.Dispose();
                while (!objectStore.IsDisposed) ;
                objectStore_ = null;
            }

            if (blocking_background_threads_)
            {
                ObjectStore.UnblockBackgroundThreads();
                blocking_background_threads_ = false;
            }
            inhibit = !reopen;
        }

        //public static void ReOpen()
        //{
        //    inhibit = false;
        //}

        //public static void Refresh()
        //{
        //    if (objectStore != null)
        //    {
        //        inhibit = true;
        //        //ObjectStore.AddObjectStoreReference();
        //        //ObjectStore.DisposeSingleton();
        //        //inhibit = false;
        //    }
        //}
    }
}
