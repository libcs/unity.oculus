namespace Oculus.Platform
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    public static class Callback
    {
        // Notification Callbacks: Exposed through Oculus.Platform.Platform

        internal static void SetNotificationCallback<T>(Message.MessageType type, Message<T>.Callback callback)
        {
            if (callback == null)
                throw new Exception("Cannot provide a null notification callback.");
            notificationCallbacks[type] = new RequestCallback<T>(callback);
            if (type == Message.MessageType.Notification_Room_InviteAccepted)
                FlushRoomInviteNotificationQueue();
        }
        internal static void SetNotificationCallback(Message.MessageType type, Message.Callback callback)
        {
            if (callback == null)
                throw new Exception("Cannot provide a null notification callback.");
            notificationCallbacks[type] = new RequestCallback(callback);
        }

        // OnComplete Callbacks: Exposed through Oculus.Platform.Request

        internal static void OnComplete<T>(Request<T> request, Message<T>.Callback callback) => requestIDsToCallbacks[request.RequestID] = new RequestCallback<T>(callback);
        internal static void OnComplete(Request request, Message.Callback callback) => requestIDsToCallbacks[request.RequestID] = new RequestCallback(callback);
        internal static void RunCallbacks()
        {
            while (true)
            {
                var msg = Message.PopMessage();
                if (msg == null)
                    break;
                HandleMessage(msg);
            }
        }
        internal static void RunLimitedCallbacks(uint limit)
        {
            for (var i = 0; i < limit; ++i)
            {
                var msg = Message.PopMessage();
                if (msg == null)
                    break;
                HandleMessage(msg);
            }
        }

        // Callback Internals

        static Dictionary<ulong, RequestCallback> requestIDsToCallbacks = new Dictionary<ulong, RequestCallback>();
        static Dictionary<Message.MessageType, RequestCallback> notificationCallbacks = new Dictionary<Message.MessageType, RequestCallback>();

        static bool hasRegisteredRoomInviteNotificationHandler = false;
        static List<Message> pendingRoomInviteNotifications = new List<Message>();
        static void FlushRoomInviteNotificationQueue()
        {
            hasRegisteredRoomInviteNotificationHandler = true;
            foreach (var msg in pendingRoomInviteNotifications)
                HandleMessage(msg);
            pendingRoomInviteNotifications.Clear();
        }

        class RequestCallback
        {
            readonly Message.Callback messageCallback;
            public RequestCallback() { }
            public RequestCallback(Message.Callback callback) => messageCallback = callback;
            public virtual void HandleMessage(Message msg) => messageCallback?.Invoke(msg);
        }

        sealed class RequestCallback<T> : RequestCallback
        {
            readonly Message<T>.Callback callback;
            public RequestCallback(Message<T>.Callback callback) => this.callback = callback;
            public override void HandleMessage(Message msg)
            {
                if (callback != null)
                {
                    // We need to queue up GameInvites because the callback runner will be called before a handler has beeen set.
                    if (!hasRegisteredRoomInviteNotificationHandler && msg.Type == Message.MessageType.Notification_Room_InviteAccepted)
                    {
                        pendingRoomInviteNotifications.Add(msg);
                        return;
                    }
                    if (msg is Message<T>) callback((Message<T>)msg);
                    else Debug.LogError($"Unable to handle message: {msg.GetType()}");
                }
            }
        }

        static void HandleMessage(Message msg)
        {
            if (requestIDsToCallbacks.TryGetValue(msg.RequestID, out var callbackHolder))
            {
                try { callbackHolder.HandleMessage(msg); }
                // even if there are exceptions, we should clean up cleanly
                finally { requestIDsToCallbacks.Remove(msg.RequestID); }
            }
            else if (notificationCallbacks.TryGetValue(msg.Type, out callbackHolder))
                callbackHolder.HandleMessage(msg);
        }
    }
}
