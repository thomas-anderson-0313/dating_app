using System;
using System.Collections.ObjectModel;
using Android.App;

using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using QuickDate.Helpers.CacheLoaders;
using QuickDate.Helpers.Utils;

namespace QuickDate.Activities.InviteFriends.Adapters
{
    public class InviteContactAdapter : RecyclerView.Adapter
    {
        public event EventHandler<InviteContactAdapterClickEventArgs> OnItemClick;
        public event EventHandler<InviteContactAdapterClickEventArgs> OnItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<Methods.PhoneContactManager.UserContact> UsersPhoneContacts = new ObservableCollection<Methods.PhoneContactManager.UserContact>();

        public InviteContactAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HContact_view
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_NotificationsView, parent, false);
                var vh = new InviteContactAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {

                if (viewHolder is InviteContactAdapterViewHolder holder)
                {
                    var item = UsersPhoneContacts[position];
                    if (item != null)
                    {
                        Initialize(holder, item);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void Initialize(InviteContactAdapterViewHolder holder, Methods.PhoneContactManager.UserContact users)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext,"no_profile_image", holder.ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                 
                string name = Methods.FunString.DecodeString(users.UserDisplayName);
                holder.UserName.Text = name;
                holder.Description.Text = users.PhoneNumber;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public override int ItemCount => UsersPhoneContacts?.Count ?? 0;

        public Methods.PhoneContactManager.UserContact GetItem(int position)
        {
            return UsersPhoneContacts[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        void Click(InviteContactAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(InviteContactAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);
    }

    public class InviteContactAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView ImageUser { get; private set; }
        public View CircleIcon { get; private set; }
        public TextView IconNotify { get; private set; }
        public TextView UserName { get; private set; }
        public TextView Description { get; private set; }

        #endregion

        public InviteContactAdapterViewHolder(View itemView, Action<InviteContactAdapterClickEventArgs> clickListener, Action<InviteContactAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                ImageUser = (ImageView)MainView.FindViewById(Resource.Id.ImageUser);
                CircleIcon = MainView.FindViewById<View>(Resource.Id.CircleIcon);
                IconNotify = (TextView)MainView.FindViewById(Resource.Id.IconNotifications);
                UserName = (TextView)MainView.FindViewById(Resource.Id.NotificationsName);
                Description = (TextView)MainView.FindViewById(Resource.Id.NotificationsText);
                 
                CircleIcon.Visibility = ViewStates.Invisible;
                IconNotify.Visibility = ViewStates.Invisible;
                 
                //Event
                itemView.Click += (sender, e) => clickListener(new InviteContactAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new InviteContactAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class InviteContactAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}