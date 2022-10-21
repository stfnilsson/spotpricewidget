using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Util;
using Android.Widget;
using Xamarin.Essentials;
using static Android.Telecom.Call;

namespace HelloAppWidget
{
    [BroadcastReceiver(Label = "HellApp Widget")]
	[IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
	[MetaData("android.appwidget.provider", Resource = "@xml/appwidgetprovider")]
	public class AppWidget : AppWidgetProvider
	{
		private static readonly string AnnouncementClick = "AnnouncementClickTag";

		private IDataFromApi _api;
        private IDataFromApi Api
        {
            get
            {
				if(_api == null)
                {
					_api = new DataApi();
                }
				return _api;
            }
        }


        public override async void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
		{
			var me = new ComponentName(context, Java.Lang.Class.FromType(typeof(AppWidget)).Name);

            var data = await Api.GetDataFromServerAsync().ConfigureAwait(false);

			if(data == null)
            {
				return;
            }
            MainThread.BeginInvokeOnMainThread(() =>
            {
                appWidgetManager.UpdateAppWidget(me, BuildRemoteViews(context, appWidgetIds, data));
            });
		}
        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);

            // Check if the click is from the "Announcement" button
            if (AnnouncementClick.Equals(intent.Action))
            {
                var pm = context.PackageManager;
                try
                {
                    var packageName = "com.android.settings";
                    var launchIntent = pm.GetLaunchIntentForPackage(packageName);
                    context.StartActivity(launchIntent);
                }
                catch
                {
                    // Something went wrong :)
                }
            }
        }

        private RemoteViews BuildRemoteViews(Context context, int[] appWidgetIds, VattenfallSpotPrice vattenfallSpotPrice)
		{
			// Retrieve the widget layout. This is a RemoteViews, so we can't use 'FindViewById'
			var widgetView = new RemoteViews(context.PackageName, Resource.Layout.Widget);

			SetTextViewText(widgetView, vattenfallSpotPrice);
			RegisterClicks(context, appWidgetIds, widgetView);

			return widgetView;
		}

		private void SetTextViewText(RemoteViews widgetView, VattenfallSpotPrice vattenfallSpotPrice)
		{
			
			widgetView.SetTextViewText(Resource.Id.widgetMedium, $"Price {vattenfallSpotPrice.Value} {vattenfallSpotPrice.Unit}");
			widgetView.SetTextViewText(Resource.Id.widgetSmall, $"Last at: {vattenfallSpotPrice.TimeStampHour}");
		}

		private void RegisterClicks(Context context, int[] appWidgetIds, RemoteViews widgetView)
		{
			var intent = new Intent(context, typeof(AppWidget));
			intent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
			intent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, appWidgetIds);

			// Register click event for the Background
			var piBackground = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);
			widgetView.SetOnClickPendingIntent(Resource.Id.widgetBackground, piBackground);

			// Register click event for the Announcement-icon
			widgetView.SetOnClickPendingIntent(Resource.Id.widgetAnnouncementIcon, GetPendingSelfIntent(context, AnnouncementClick));
		}

		private PendingIntent GetPendingSelfIntent(Context context, string action)
		{
			var intent = new Intent(context, typeof(AppWidget));
			intent.SetAction(action);
			return PendingIntent.GetBroadcast(context, 0, intent, 0);
		}

	}
}
