﻿using Reddit.Controllers.EventArgs;
using Reddit.Controllers.Internal;
using Reddit.Controllers.Structures;
using Reddit.Exceptions;
using Reddit.Models.Inputs;
using Reddit.Models.Inputs.Listings;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Reddit.Controllers
{
    /// <summary>
    /// Controller class for subreddit post listings.
    /// </summary>
    public class SubredditPosts : Monitors
    {
        public event EventHandler<PostsUpdateEventArgs> BestUpdated;
        public event EventHandler<PostsUpdateEventArgs> HotUpdated;
        public event EventHandler<PostsUpdateEventArgs> NewUpdated;
        public event EventHandler<PostsUpdateEventArgs> RisingUpdated;
        public event EventHandler<PostsUpdateEventArgs> TopUpdated;
        public event EventHandler<PostsUpdateEventArgs> ControversialUpdated;

        public event EventHandler<PostsUpdateEventArgs> ModQueueUpdated;
        public event EventHandler<PostsUpdateEventArgs> ModQueueReportsUpdated;
        public event EventHandler<PostsUpdateEventArgs> ModQueueSpamUpdated;
        public event EventHandler<PostsUpdateEventArgs> ModQueueUnmoderatedUpdated;
        public event EventHandler<PostsUpdateEventArgs> ModQueueEditedUpdated;

        internal override ref Models.Internal.Monitor MonitorModel => ref Dispatch.Monitor;
        internal override ref MonitoringSnapshot Monitoring => ref MonitorModel.Monitoring;

        /// <summary>
        /// List of posts using "best" sort.
        /// </summary>
        public List<Post> Best
        {
            get
            {
                return (BestLastUpdated.HasValue
                    && BestLastUpdated.Value.AddSeconds(15) > DateTime.Now ? best : GetBest());
            }
            private set
            {
                best = value;
            }
        }
        internal List<Post> best;

        /// <summary>
        /// List of posts using "hot" sort.
        /// </summary>
        public List<Post> Hot
        {
            get
            {
                return (HotLastUpdated.HasValue
                    && HotLastUpdated.Value.AddSeconds(15) > DateTime.Now ? hot : GetHot());
            }
            private set
            {
                hot = value;
            }
        }
        internal List<Post> hot;

        /// <summary>
        /// List of posts using "new" sort.
        /// </summary>
        public List<Post> New
        {
            get
            {
                return (NewLastUpdated.HasValue
                    && NewLastUpdated.Value.AddSeconds(15) > DateTime.Now ? newPosts : GetNew());
            }
            private set
            {
                newPosts = value;
            }
        }
        internal List<Post> newPosts;

        /// <summary>
        /// List of posts using "rising" sort.
        /// </summary>
        public List<Post> Rising
        {
            get
            {
                return (RisingLastUpdated.HasValue
                    && RisingLastUpdated.Value.AddSeconds(15) > DateTime.Now ? rising : GetRising());
            }
            private set
            {
                rising = value;
            }
        }
        internal List<Post> rising;

        /// <summary>
        /// List of posts using "top" sort.
        /// </summary>
        public List<Post> Top
        {
            get
            {
                return (TopLastUpdated.HasValue
                    && TopLastUpdated.Value.AddSeconds(15) > DateTime.Now ? top : GetTop(TopT));
            }
            private set
            {
                top = value;
            }
        }
        internal List<Post> top;

        /// <summary>
        /// List of posts using "controversial" sort.
        /// </summary>
        public List<Post> Controversial
        {
            get
            {
                return (ControversialLastUpdated.HasValue
                    && ControversialLastUpdated.Value.AddSeconds(15) > DateTime.Now ? controversial : GetControversial(ControversialT));
            }
            private set
            {
                controversial = value;
            }
        }
        internal List<Post> controversial;

        /// <summary>
        /// List of posts in the mod queue.
        /// </summary>
        public List<Post> ModQueue
        {
            get
            {
                return (ModQueueLastUpdated.HasValue
                    && ModQueueLastUpdated.Value.AddSeconds(15) > DateTime.Now ? modQueue : GetModQueue());
            }
            private set
            {
                modQueue = value;
            }
        }
        internal List<Post> modQueue;

        /// <summary>
        /// List of reported posts in the mod queue.
        /// </summary>
        public List<Post> ModQueueReports
        {
            get
            {
                return (ModQueueLastUpdated.HasValue
                    && ModQueueLastUpdated.Value.AddSeconds(15) > DateTime.Now ? modQueueReports : GetModQueue());
            }
            private set
            {
                modQueueReports = value;
            }
        }
        internal List<Post> modQueueReports;

        /// <summary>
        /// List of spammed posts in the mod queue.
        /// </summary>
        public List<Post> ModQueueSpam
        {
            get
            {
                return (ModQueueLastUpdated.HasValue
                    && ModQueueLastUpdated.Value.AddSeconds(15) > DateTime.Now ? modQueueSpam : GetModQueue());
            }
            private set
            {
                modQueueSpam = value;
            }
        }
        internal List<Post> modQueueSpam;

        /// <summary>
        /// List of unmoderated posts in the mod queue.
        /// </summary>
        public List<Post> ModQueueUnmoderated
        {
            get
            {
                return (ModQueueLastUpdated.HasValue
                    && ModQueueLastUpdated.Value.AddSeconds(15) > DateTime.Now ? modQueueUnmoderated : GetModQueue());
            }
            private set
            {
                modQueueUnmoderated = value;
            }
        }
        internal List<Post> modQueueUnmoderated;

        /// <summary>
        /// List of edited posts in the mod queue.
        /// </summary>
        public List<Post> ModQueueEdited
        {
            get
            {
                return (ModQueueLastUpdated.HasValue
                    && ModQueueLastUpdated.Value.AddSeconds(15) > DateTime.Now ? modQueueEdited : GetModQueue());
            }
            private set
            {
                modQueueEdited = value;
            }
        }
        internal List<Post> modQueueEdited;

        private DateTime? BestLastUpdated;
        private DateTime? HotLastUpdated;
        private DateTime? NewLastUpdated;
        private DateTime? RisingLastUpdated;
        private DateTime? TopLastUpdated;
        private DateTime? ControversialLastUpdated;

        private DateTime? ModQueueLastUpdated;
        private DateTime? ModQueueReportsLastUpdated;
        private DateTime? ModQueueSpamLastUpdated;
        private DateTime? ModQueueUnmoderatedLastUpdated;
        private DateTime? ModQueueEditedLastUpdated;

        private readonly string Subreddit;
        private Dispatch Dispatch;

        private string TopT = "all";
        private string ControversialT = "all";

        /// <summary>
        /// Create a new instance of the subreddit posts controller.
        /// </summary>
        /// <param name="dispatch"></param>
        /// <param name="subreddit">The name of the subreddit</param>
        /// <param name="best"></param>
        /// <param name="hot"></param>
        /// <param name="newPosts"></param>
        /// <param name="rising"></param>
        /// <param name="top"></param>
        /// <param name="controversial"></param>
        /// <param name="modQueue"></param>
        /// <param name="modQueueReports"></param>
        /// <param name="modQueueSpam"></param>
        /// <param name="modQueueUnmoderated"></param>
        /// <param name="modQueueEdited"></param>
        public SubredditPosts(ref Dispatch dispatch, string subreddit, List<Post> best = null, List<Post> hot = null, List<Post> newPosts = null,
            List<Post> rising = null, List<Post> top = null, List<Post> controversial = null, List<Post> modQueue = null, 
            List<Post> modQueueReports = null, List<Post> modQueueSpam = null, List<Post> modQueueUnmoderated = null, 
            List<Post> modQueueEdited = null) 
            : base()
        {
            Dispatch = dispatch;
            Subreddit = subreddit;

            Best = best ?? new List<Post>();
            Hot = hot ?? new List<Post>();
            New = newPosts ?? new List<Post>();
            Rising = rising ?? new List<Post>();
            Top = top ?? new List<Post>();
            Controversial = controversial ?? new List<Post>();

            ModQueue = modQueue ?? new List<Post>();
            ModQueueReports = modQueueReports ?? new List<Post>();
            ModQueueSpam = modQueueSpam ?? new List<Post>();
            ModQueueUnmoderated = modQueueUnmoderated ?? new List<Post>();
            ModQueueEdited = modQueueEdited ?? new List<Post>();
        }

        /// <summary>
        /// Retrieve a list of posts using "best" sort.
        /// </summary>
        /// <param name="after">fullname of a thing</param>
        /// <param name="before">fullname of a thing</param>
        /// <param name="limit"></param>
        /// <returns>A list of posts.</returns>
        public List<Post> GetBest(string after = "", string before = "", int limit = 100)
        {
            List<Post> posts = Listings.GetPosts(Dispatch.Listings.Best(new CategorizedSrListingInput(after, before, limit: limit)), Dispatch);

            BestLastUpdated = DateTime.Now;

            Best = posts;
            return posts;
        }

        /// <summary>
        /// Retrieve a list of posts using "hot" sort.
        /// </summary>
        /// <param name="after">fullname of a thing</param>
        /// <param name="before">fullname of a thing</param>
        /// <param name="limit"></param>
        /// <returns>A list of posts.</returns>
        public List<Post> GetHot(string g = "", string after = "", string before = "", int limit = 100)
        {
            List<Post> posts = Listings.GetPosts(Dispatch.Listings.Hot(new ListingsHotInput(g, after, before, limit: limit), Subreddit), Dispatch);

            HotLastUpdated = DateTime.Now;

            Hot = posts;
            return posts;
        }

        /// <summary>
        /// Retrieve a list of posts using "new" sort.
        /// </summary>
        /// <param name="after">fullname of a thing</param>
        /// <param name="before">fullname of a thing</param>
        /// <param name="limit"></param>
        /// <returns>A list of posts.</returns>
        public List<Post> GetNew(string after = "", string before = "", int limit = 100)
        {
            List<Post> posts = Listings.GetPosts(Dispatch.Listings.New(new CategorizedSrListingInput(after, before, limit: limit), Subreddit), Dispatch);

            NewLastUpdated = DateTime.Now;

            New = posts;
            return posts;
        }

        /// <summary>
        /// Retrieve a list of posts using "rising" sort.
        /// </summary>
        /// <param name="after">fullname of a thing</param>
        /// <param name="before">fullname of a thing</param>
        /// <param name="limit"></param>
        /// <returns>A list of posts.</returns>
        public List<Post> GetRising(string after = "", string before = "", int limit = 100)
        {
            List<Post> posts = Listings.GetPosts(Dispatch.Listings.Rising(new CategorizedSrListingInput(after, before, limit: limit), Subreddit), Dispatch);

            RisingLastUpdated = DateTime.Now;

            Rising = posts;
            return posts;
        }

        /// <summary>
        /// Retrieve a list of posts using "top" sort.
        /// </summary>
        /// <param name="after">fullname of a thing</param>
        /// <param name="before">fullname of a thing</param>
        /// <param name="limit"></param>
        /// <returns>A list of posts.</returns>
        public List<Post> GetTop(string t = "all", string after = "", string before = "", int limit = 100)
        {
            List<Post> posts = Listings.GetPosts(Dispatch.Listings.Top(new TimedCatSrListingInput(t, after, before, limit: limit), Subreddit), Dispatch);

            TopLastUpdated = DateTime.Now;

            Top = posts;
            TopT = t;
            return posts;
        }

        /// <summary>
        /// Retrieve a list of posts using "controversial" sort.
        /// </summary>
        /// <param name="after">fullname of a thing</param>
        /// <param name="before">fullname of a thing</param>
        /// <param name="limit"></param>
        /// <returns>A list of posts.</returns>
        public List<Post> GetControversial(string t = "all", string after = "", string before = "", int limit = 100)
        {
            List<Post> posts = Listings.GetPosts(Dispatch.Listings.Controversial(new TimedCatSrListingInput(t, after, before, limit: limit), Subreddit), Dispatch);

            ControversialLastUpdated = DateTime.Now;

            Controversial = posts;
            ControversialT = t;
            return posts;
        }

        private List<Post> GetModQueuePosts(string location, string after = "", string before = "", int limit = 100, string show = "all",
            bool srDetail = false, int count = 0)
        {
            return Listings.GetPosts(Dispatch.Moderation.ModQueue(location, after, before, "links", Subreddit, count, limit, show, srDetail), Dispatch);
        }

        /// <summary>
        /// Retrieve a list of posts in the mod queue.
        /// </summary>
        /// <param name="after"></param>
        /// <param name="before"></param>
        /// <param name="limit"></param>
        /// <param name="show"></param>
        /// <param name="srDetail"></param>
        /// <param name="count"></param>
        /// <returns>A list of posts.</returns>
        public List<Post> GetModQueue(string after = "", string before = "", int limit = 100, string show = "all", bool srDetail = false, int count = 0)
        {
            List<Post> posts = GetModQueuePosts("modqueue", after, before, limit, show, srDetail, count);

            ModQueueLastUpdated = DateTime.Now;

            ModQueue = posts;
            return posts;
        }

        /// <summary>
        /// Retrieve a list of reported posts in the mod queue.
        /// </summary>
        /// <param name="after"></param>
        /// <param name="before"></param>
        /// <param name="limit"></param>
        /// <param name="show"></param>
        /// <param name="srDetail"></param>
        /// <param name="count"></param>
        /// <returns>A list of posts.</returns>
        public List<Post> GetModQueueReports(string after = "", string before = "", int limit = 100, string show = "all", bool srDetail = false, int count = 0)
        {
            List<Post> posts = GetModQueuePosts("reports", after, before, limit, show, srDetail, count);

            ModQueueReportsLastUpdated = DateTime.Now;

            ModQueueReports = posts;
            return posts;
        }

        /// <summary>
        /// Retrieve a list of spammed posts in the mod queue.
        /// </summary>
        /// <param name="after"></param>
        /// <param name="before"></param>
        /// <param name="limit"></param>
        /// <param name="show"></param>
        /// <param name="srDetail"></param>
        /// <param name="count"></param>
        /// <returns>A list of posts.</returns>
        public List<Post> GetModQueueSpam(string after = "", string before = "", int limit = 100, string show = "all", bool srDetail = false, int count = 0)
        {
            List<Post> posts = GetModQueuePosts("spam", after, before, limit, show, srDetail, count);

            ModQueueSpamLastUpdated = DateTime.Now;

            ModQueueSpam = posts;
            return posts;
        }

        /// <summary>
        /// Retrieve a list of unmoderated posts in the mod queue.
        /// </summary>
        /// <param name="after"></param>
        /// <param name="before"></param>
        /// <param name="limit"></param>
        /// <param name="show"></param>
        /// <param name="srDetail"></param>
        /// <param name="count"></param>
        /// <returns>A list of posts.</returns>
        public List<Post> GetModQueueUnmoderated(string after = "", string before = "", int limit = 100, string show = "all", bool srDetail = false, int count = 0)
        {
            List<Post> posts = GetModQueuePosts("unmoderated", after, before, limit, show, srDetail, count);

            ModQueueUnmoderatedLastUpdated = DateTime.Now;

            ModQueueUnmoderated = posts;
            return posts;
        }

        /// <summary>
        /// Retrieve a list of edited posts in the mod queue.
        /// </summary>
        /// <param name="after"></param>
        /// <param name="before"></param>
        /// <param name="limit"></param>
        /// <param name="show"></param>
        /// <param name="srDetail"></param>
        /// <param name="count"></param>
        /// <returns>A list of posts.</returns>
        public List<Post> GetModQueueEdited(string after = "", string before = "", int limit = 100, string show = "all", bool srDetail = false, int count = 0)
        {
            List<Post> posts = GetModQueuePosts("edited", after, before, limit, show, srDetail, count);

            ModQueueEditedLastUpdated = DateTime.Now;

            ModQueueEdited = posts;
            return posts;
        }

        /// <summary>
        /// Monitor Reddit for new "Best" posts.
        /// </summary>
        /// <returns>True if this action turned monitoring on, false if this action turned it off.</returns>
        public bool MonitorBest()
        {
            string key = "BestPosts";
            return Monitor(key, new Thread(() => MonitorBestThread(key)), Subreddit);
        }

        private void MonitorBestThread(string key)
        {
            MonitorPostsThread(Monitoring, key, "best", Subreddit);
        }

        internal virtual void OnBestUpdated(PostsUpdateEventArgs e)
        {
            BestUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Monitor the subreddit for new "Hot" posts.
        /// </summary>
        /// <returns>True if this action turned monitoring on, false if this action turned it off.</returns>
        public bool MonitorHot()
        {
            string key = "HotPosts";
            return Monitor(key, new Thread(() => MonitorHotThread(key)), Subreddit);
        }

        private void MonitorHotThread(string key)
        {
            MonitorPostsThread(Monitoring, key, "hot", Subreddit);
        }

        internal virtual void OnHotUpdated(PostsUpdateEventArgs e)
        {
            HotUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Monitor the subreddit for new posts.
        /// </summary>
        /// <returns>True if this action turned monitoring on, false if this action turned it off.</returns>
        public bool MonitorNew()
        {
            string key = "NewPosts";
            return Monitor(key, new Thread(() => MonitorNewThread(key)), Subreddit);
        }

        private void MonitorNewThread(string key)
        {
            MonitorPostsThread(Monitoring, key, "new", Subreddit);
        }

        internal virtual void OnNewUpdated(PostsUpdateEventArgs e)
        {
            NewUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Monitor the subreddit for new "Rising" posts.
        /// </summary>
        /// <returns>True if this action turned monitoring on, false if this action turned it off.</returns>
        public bool MonitorRising()
        {
            string key = "RisingPosts";
            return Monitor(key, new Thread(() => MonitorRisingThread(key)), Subreddit);
        }

        private void MonitorRisingThread(string key)
        {
            MonitorPostsThread(Monitoring, key, "rising", Subreddit);
        }

        internal virtual void OnRisingUpdated(PostsUpdateEventArgs e)
        {
            RisingUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Monitor the subreddit for new "Top" posts.
        /// </summary>
        /// <returns>True if this action turned monitoring on, false if this action turned it off.</returns>
        public bool MonitorTop()
        {
            string key = "TopPosts";
            return Monitor(key, new Thread(() => MonitorTopThread(key)), Subreddit);
        }

        private void MonitorTopThread(string key)
        {
            MonitorPostsThread(Monitoring, key, "top", Subreddit);
        }

        internal virtual void OnTopUpdated(PostsUpdateEventArgs e)
        {
            TopUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Monitor the subreddit for new "Controversial" posts.
        /// </summary>
        /// <returns>True if this action turned monitoring on, false if this action turned it off.</returns>
        public bool MonitorControversial()
        {
            string key = "ControversialPosts";
            return Monitor(key, new Thread(() => MonitorControversialThread(key)), Subreddit);
        }

        private void MonitorControversialThread(string key)
        {
            MonitorPostsThread(Monitoring, key, "controversial", Subreddit);
        }

        internal virtual void OnControversialUpdated(PostsUpdateEventArgs e)
        {
            ControversialUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Monitor the subreddit's modqueue for new "modqueue" posts.
        /// </summary>
        /// <returns>True if this action turned monitoring on, false if this action turned it off.</returns>
        public bool MonitorModQueue()
        {
            string key = "ModQueuePosts";
            return Monitor(key, new Thread(() => MonitorModQueueThread(key)), Subreddit);
        }

        private void MonitorModQueueThread(string key)
        {
            MonitorPostsThread(Monitoring, key, "modqueue", Subreddit);
        }

        internal virtual void OnModQueueUpdated(PostsUpdateEventArgs e)
        {
            ModQueueUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Monitor the subreddit's modqueue for new "reports" posts.
        /// </summary>
        /// <returns>True if this action turned monitoring on, false if this action turned it off.</returns>
        public bool MonitorModQueueReports()
        {
            string key = "ModQueueReportsPosts";
            return Monitor(key, new Thread(() => MonitorModQueueReportsThread(key)), Subreddit);
        }

        private void MonitorModQueueReportsThread(string key)
        {
            MonitorPostsThread(Monitoring, key, "modqueuereports", Subreddit);
        }

        internal virtual void OnModQueueReportsUpdated(PostsUpdateEventArgs e)
        {
            ModQueueReportsUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Monitor the subreddit's modqueue for new "spam" posts.
        /// </summary>
        /// <returns>True if this action turned monitoring on, false if this action turned it off.</returns>
        public bool MonitorModQueueSpam()
        {
            string key = "ModQueueSpamPosts";
            return Monitor(key, new Thread(() => MonitorModQueueSpamThread(key)), Subreddit);
        }

        private void MonitorModQueueSpamThread(string key)
        {
            MonitorPostsThread(Monitoring, key, "modqueuespam", Subreddit);
        }

        internal virtual void OnModQueueSpamUpdated(PostsUpdateEventArgs e)
        {
            ModQueueSpamUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Monitor the subreddit's modqueue for new "unmoderated" posts.
        /// </summary>
        /// <returns>True if this action turned monitoring on, false if this action turned it off.</returns>
        public bool MonitorModQueueUnmoderated()
        {
            string key = "ModQueueUnmoderatedPosts";
            return Monitor(key, new Thread(() => MonitorModQueueUnmoderatedThread(key)), Subreddit);
        }

        private void MonitorModQueueUnmoderatedThread(string key)
        {
            MonitorPostsThread(Monitoring, key, "modqueueunmoderated", Subreddit);
        }

        internal virtual void OnModQueueUnmoderatedUpdated(PostsUpdateEventArgs e)
        {
            ModQueueUnmoderatedUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Monitor the subreddit's modqueue for new "edited" posts.
        /// </summary>
        /// <returns>True if this action turned monitoring on, false if this action turned it off.</returns>
        public bool MonitorModQueueEdited()
        {
            string key = "ModQueueEditedPosts";
            return Monitor(key, new Thread(() => MonitorModQueueEditedThread(key)), Subreddit);
        }

        private void MonitorModQueueEditedThread(string key)
        {
            MonitorPostsThread(Monitoring, key, "modqueueedited", Subreddit);
        }

        internal virtual void OnModQueueEditedUpdated(PostsUpdateEventArgs e)
        {
            ModQueueEditedUpdated?.Invoke(this, e);
        }

        protected override Thread CreateMonitoringThread(string key, string subKey, int startDelayMs = 0)
        {
            switch (key)
            {
                default:
                    throw new RedditControllerException("Unrecognized key.");
                case "BestPosts":
                    return new Thread(() => MonitorPostsThread(Monitoring, key, "best", subKey, startDelayMs));
                case "HotPosts":
                    return new Thread(() => MonitorPostsThread(Monitoring, key, "hot", subKey, startDelayMs));
                case "NewPosts":
                    return new Thread(() => MonitorPostsThread(Monitoring, key, "new", subKey, startDelayMs));
                case "RisingPosts":
                    return new Thread(() => MonitorPostsThread(Monitoring, key, "rising", subKey, startDelayMs));
                case "TopPosts":
                    return new Thread(() => MonitorPostsThread(Monitoring, key, "top", subKey, startDelayMs));
                case "ControversialPosts":
                    return new Thread(() => MonitorPostsThread(Monitoring, key, "controversial", subKey, startDelayMs));
                case "ModQueuePosts":
                    return new Thread(() => MonitorPostsThread(Monitoring, key, "modqueue", subKey, startDelayMs));
                case "ModQueueReportsPosts":
                    return new Thread(() => MonitorPostsThread(Monitoring, key, "modqueuereports", subKey, startDelayMs));
                case "ModQueueSpamPosts":
                    return new Thread(() => MonitorPostsThread(Monitoring, key, "modqueuespam", subKey, startDelayMs));
                case "ModQueueUnmoderatedPosts":
                    return new Thread(() => MonitorPostsThread(Monitoring, key, "modqueueunmoderated", subKey, startDelayMs));
                case "ModQueueEditedPosts":
                    return new Thread(() => MonitorPostsThread(Monitoring, key, "modqueueedited", subKey, startDelayMs));
            }
        }

        private void MonitorPostsThread(MonitoringSnapshot monitoring, string key, string type, string subKey, int startDelayMs = 0)
        {
            if (startDelayMs > 0)
            {
                Thread.Sleep(startDelayMs);
            }

            while (!Terminate
                && Monitoring.Get(key).Contains(subKey))
            {
                List<Post> oldList;
                List<Post> newList;
                switch (type)
                {
                    default:
                        throw new RedditControllerException("Unrecognized type '" + type + "'.");
                    case "best":
                        oldList = best;
                        newList = GetBest();
                        break;
                    case "hot":
                        oldList = hot;
                        newList = GetHot();
                        break;
                    case "new":
                        oldList = newPosts;
                        newList = GetNew();
                        break;
                    case "rising":
                        oldList = rising;
                        newList = GetRising();
                        break;
                    case "top":
                        oldList = top;
                        newList = GetTop();
                        break;
                    case "controversial":
                        oldList = controversial;
                        newList = GetControversial();
                        break;
                    case "modqueue":
                        oldList = modQueue;
                        newList = GetModQueue();
                        break;
                    case "modqueuereports":
                        oldList = modQueueReports;
                        newList = GetModQueueReports();
                        break;
                    case "modqueuespam":
                        oldList = modQueueSpam;
                        newList = GetModQueueSpam();
                        break;
                    case "modqueueunmoderated":
                        oldList = modQueueUnmoderated;
                        newList = GetModQueueUnmoderated();
                        break;
                    case "modqueueedited":
                        oldList = modQueueEdited;
                        newList = GetModQueueEdited();
                        break;
                }

                if (Listings.ListDiff(oldList, newList, out List<Post> added, out List<Post> removed))
                {
                    // Event handler to alert the calling app that the list has changed.  --Kris
                    PostsUpdateEventArgs args = new PostsUpdateEventArgs
                    {
                        NewPosts = newList,
                        OldPosts = oldList,
                        Added = added,
                        Removed = removed
                    };
                    TriggerUpdate(args, type);
                }

                Thread.Sleep(Monitoring.Count() * MonitoringWaitDelayMS);
            }
        }

        private void TriggerUpdate(PostsUpdateEventArgs args, string type)
        {
            switch (type)
            {
                case "best":
                    OnBestUpdated(args);
                    break;
                case "hot":
                    OnHotUpdated(args);
                    break;
                case "new":
                    OnNewUpdated(args);
                    break;
                case "rising":
                    OnRisingUpdated(args);
                    break;
                case "top":
                    OnTopUpdated(args);
                    break;
                case "controversial":
                    OnControversialUpdated(args);
                    break;
                case "modqueue":
                    OnModQueueUpdated(args);
                    break;
                case "modqueuereports":
                    OnModQueueReportsUpdated(args);
                    break;
                case "modqueuespam":
                    OnModQueueSpamUpdated(args);
                    break;
                case "modqueueunmoderated":
                    OnModQueueUnmoderatedUpdated(args);
                    break;
                case "modqueueedited":
                    OnModQueueEditedUpdated(args);
                    break;
            }
        }
    }
}
