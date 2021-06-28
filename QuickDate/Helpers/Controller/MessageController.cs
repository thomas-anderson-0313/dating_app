﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Widget;
using QuickDate.Activities.Chat;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDate.SQLite;
using QuickDateClient.Classes.Chat;
using QuickDateClient.Classes.Global;
using QuickDateClient.Requests;
using QuickDateClient;

namespace QuickDate.Helpers.Controller
{
    public static class MessageController
    {
        //############# DON'T MODIFY HERE #############
        //========================= Functions =========================

        public static async Task SendMessageTask(Activity activity, int userId, string text, string stickerId, string path, string hashId, UserInfoObject userData)
        {
            var (apiStatus, respond) = await RequestsAsync.Chat.SendMessageAsync(userId.ToString(), text, stickerId, path, hashId);
            if (apiStatus == 200)
            {
                if (respond is SendMessageObject result)
                {
                    if (result.Data != null)
                    {
                        activity?.RunOnUiThread(() =>
                        {
                            try
                            {
                                UpdateLastIdMessage(result, userData, hashId);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                }
            }
            else
            {
                Methods.DisplayReportResult(activity, respond);
                if (respond is ErrorObject error)
                {
                    var errorText = error.ErrorData.ErrorText;
                    Toast.MakeText(Application.Context, errorText, ToastLength.Short);
                }
            }
        }

        private static void UpdateLastIdMessage(SendMessageObject messages, UserInfoObject userData, string hashId)
        {
            try
            {
                var checker = MessagesBoxActivity.MAdapter?.MessageList?.FirstOrDefault(a => a.Id == Convert.ToInt32(hashId));
                if (checker != null)
                {
                    checker.Id = messages.Data.Id;
                    checker.FromName = UserDetails.FullName;
                    checker.FromAvater = UserDetails.Avatar;
                    checker.ToName = userData?.Fullname ?? "";
                    checker.ToAvater = userData?.Avater.PurpleUri ?? "";
                    checker.From = messages.Data.From;
                    checker.To = messages.Data.To;
                    checker.Text = Methods.FunString.DecodeString(messages.Data.Text);
                    checker.Media = checker.Media.Contains("upload/chat/") && !messages.Data.Media.Contains(Client.WebsiteUrl) ? Client.WebsiteUrl + "/" + messages.Data.Media : messages.Data.Media;
                    checker.FromDelete = messages.Data.FromDelete;
                    checker.ToDelete = messages.Data.ToDelete;
                    checker.Sticker = messages.Data.Sticker;
                    //checker.CreatedAt = messages.Data.CreatedAt;
                    checker.Seen = messages.Data.Seen;
                    checker.Type = "Sent";
                    checker.MessageType = messages.Data.MessageType;

                    string text = "";
                    switch (checker.MessageType)
                    {
                        case "text":
                            text = string.IsNullOrEmpty(text) ? Application.Context.GetText(Resource.String.Lbl_SendMessage) : Methods.FunString.DecodeString(messages.Data.Text);
                            break;
                        case "media":
                            text = Application.Context.GetText(Resource.String.Lbl_SendImageFile);
                            break;
                        case "sticker" when checker.Sticker.Contains(".gif"):
                            text = Application.Context.GetText(Resource.String.Lbl_SendGifFile);
                            break;
                        case "sticker":
                            text = Application.Context.GetText(Resource.String.Lbl_SendStickerFile);
                            break;
                        default:
                            text = Methods.FunString.DecodeString(messages.Data.Text);
                            break;
                    }

                    var dataUser = LastChatActivity.MAdapter?.UserList?.FirstOrDefault(a => a.User.Id == messages.Data.To);
                    if (dataUser != null)
                    {
                        var index = LastChatActivity.MAdapter?.UserList?.IndexOf(LastChatActivity.MAdapter.UserList?.FirstOrDefault(x => x.User.Id == messages.Data.To));
                        if (index > -1)
                        {
                            LastChatActivity.MAdapter?.UserList?.Move(Convert.ToInt32(index), 0);
                            LastChatActivity.MAdapter?.NotifyItemMoved(Convert.ToInt32(index), 0);

                            var data = LastChatActivity.MAdapter?.UserList?.FirstOrDefault(a => a.User.Id == dataUser.User.Id);
                            if (data != null)
                            {
                                data.Id = dataUser.Id;
                                data.Owner = dataUser.Owner;
                                data.User = dataUser.User;
                                data.Seen = 1;
                                data.Text = dataUser.Text;
                                data.Media = dataUser.Media;
                                data.Sticker = dataUser.Sticker;
                                data.Time = dataUser.Time;
                                data.CreatedAt = dataUser.CreatedAt;
                                data.NewMessages = dataUser.NewMessages;
                                data.MessageType = dataUser.MessageType;
                                data.FromId = dataUser.FromId;
                                data.ToId = dataUser.ToId;
                                data.MessageType = messages.Data.MessageType;

                                LastChatActivity.MAdapter.NotifyDataSetChanged();
                            }
                        }
                    }
                    else
                    {
                        if (userData != null)
                        {
                            LastChatActivity.MAdapter?.UserList?.Insert(0, new GetConversationListObject.DataConversation
                            {
                                Id = userData.Id,
                                Owner = 0,
                                User = userData,
                                Seen = 1,
                                Text = text,
                                Media = messages.Data.Media,
                                Sticker = messages.Data.Sticker,
                                Time = messages.Data.CreatedAt,
                                CreatedAt = messages.Data.CreatedAt,
                                NewMessages = 0,
                                FromId = messages.Data.From,
                                ToId = messages.Data.To,
                                MessageType = messages.Data.MessageType
                            });

                            LastChatActivity.MAdapter?.NotifyItemInserted(0);
                        }
                    }

                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    //GetChatConversationsObject.Messages message = new GetChatConversationsObject.Messages
                    //{
                    //    Id = messages.Data.Id,
                    //    FromName = UserDetails.FullName,
                    //    FromAvater = UserDetails.Avatar,
                    //    ToName = userData?.Fullname ?? "",
                    //    ToAvater = userData?.Avater ?? "",
                    //    From = messages.Data.From,
                    //    To = messages.Data.To,
                    //    Text = Methods.FunString.DecodeString(checker.Text),
                    //    Media = checker.Media,
                    //    FromDelete = checker.FromDelete,
                    //    ToDelete = checker.ToDelete,
                    //    Sticker = checker.Sticker,
                    //    //CreatedAt = checker.CreatedAt,
                    //    Seen = 1,
                    //    Type = "Sent",
                    //    MessageType = messages.Data.MessageType,
                    //};
                    //Update All data users to database
                    dbDatabase.InsertOrUpdateToOneMessages(checker);
                    

                    MessagesBoxActivity.UpdateOneMessage(checker);
                    MessagesBoxActivity.GetInstance()?.ChatBoxRecyclerView.ScrollToPosition(MessagesBoxActivity.MAdapter.MessageList.IndexOf(MessagesBoxActivity.MAdapter.MessageList.Last()));

                    if (AppSettings.RunSoundControl)
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_SendMesseges.mp3");
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}