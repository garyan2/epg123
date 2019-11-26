using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.MediaCenter.Guide;
using Microsoft.MediaCenter.Store;

namespace epg123
{
    public static class Store
    {
        private static ObjectStore objectStore_;
        private static MergedLineup mergedLineup_;
        private static ObjectStore singletonStore_;
        private static MergedLineup singletonLineup_;
        //private static bool blocking_background_threads_ = false;

        public static ObjectStore objectStore
        {
            get
            {
                if (objectStore_ == null)
                {
                    //if (!blocking_background_threads_)
                    //{
                    //    ObjectStore.WaitForThenBlockBackgroundThreads(0x7ffffff);
                    //    blocking_background_threads_ = true;
                    //}

                    SHA256Managed sha256Man = new SHA256Managed();
                    string clientId = ObjectStore.GetClientId(true);
                    string providerName = @"Anonymous!User";
                    string password = Convert.ToBase64String(sha256Man.ComputeHash(Encoding.Unicode.GetBytes(clientId)));
                    objectStore_ = ObjectStore.Open(null, providerName, password, true);
                }
                return objectStore_;
            }
        }
        public static MergedLineup mergedLineup
        {
            get
            {
                if (objectStore != null && mergedLineup_ == null)
                {
                    using (MergedLineups mergedLineups = new MergedLineups(objectStore))
                    {
                        foreach (MergedLineup lineup in mergedLineups)
                        {
                            if (lineup.GetChannels().Length > 0)
                            {
                                mergedLineup_ = lineup;
                                break;
                            }
                        }
                    }
                }
                return mergedLineup_;
            }
        }

        public static ObjectStore singletonStore
        {
            get
            {
                if (singletonStore_ == null)
                {
                    singletonStore_ = ObjectStore.DefaultSingleton;
                }
                return singletonStore_;
            }
        }
        public static MergedLineup singletonLineup
        {
            get
            {
                if (singletonStore != null && singletonLineup_ == null)
                {
                    using (MergedLineups mergedLineups = new MergedLineups(singletonStore))
                    {
                        foreach (MergedLineup lineup in mergedLineups)
                        {
                            if (lineup.GetChannels().Length > 0)
                            {
                                singletonLineup_ = lineup;
                                break;
                            }
                        }
                    }
                }
                return singletonLineup_;
            }
        }

        public static void Close(bool dispose = false)
        {
            mergedLineup_ = null;
            singletonLineup_ = null;

            if (objectStore_ != null)
            {
                //if (blocking_background_threads_)
                //{
                //    ObjectStore.UnblockBackgroundThreads();
                //    blocking_background_threads_ = false;
                //}
            }
            if (dispose)
            {
                if (objectStore_ != null)
                {
                    objectStore.Dispose();
                }
                if (singletonStore_ != null)
                {
                    ObjectStore.DisposeSingleton();
                }
            }
            objectStore_ = null;
            singletonStore_ = null;
        }
    }
}