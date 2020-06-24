﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using MySubscriptions.Model;
using MySubscriptions.ViewModel.Helpers;
using Xamarin.Forms;

[assembly: Dependency(typeof(MySubscriptions.iOS.Dependencies.Firestore))]
namespace MySubscriptions.iOS.Dependencies
{
    public class Firestore : IFirestore
    {
        public Firestore() { }

        public bool InsertSubscription(Subscription subscription)
        {
            try
            {
                var keys = new[] 
                {
                    new NSString("author"),
                    new NSString("name"),
                    new NSString("isActive"),
                    new NSString("subscribedDate")
                };

                var values = new NSObject[]
                {
                    new NSString(Firebase.Auth.Auth.DefaultInstance.CurrentUser.Uid),
                    new NSString(subscription.Name),
                    new NSNumber(subscription.IsActive),
                    DateTimeToNSDate(subscription.SubscribedDate)
                };

                var subscriptionDocument = new NSDictionary<NSString, NSObject>(keys, values);
                Firebase.CloudFirestore.Firestore.SharedInstance.GetCollection("subscriptions").AddDocument(subscriptionDocument);
                return true;
            }
            catch (Exception ex) 
            { 
                return false; 
            }
        }

        public async Task<IList<Subscription>> ReadSubscription()
        {
            try 
            {
                var collection = Firebase.CloudFirestore.Firestore.SharedInstance.GetCollection("subscriptions");
                var query = collection.WhereEqualsTo("author", Firebase.Auth.Auth.DefaultInstance.CurrentUser.Uid);
                var documents = await query.GetDocumentsAsync();

                List<Subscription> subscriptions = new List<Subscription>();
                foreach (var doc in documents.Documents)
                {
                    var subscriptionDictionary = doc.Data;
                    var subscription = new Subscription
                    {
                        IsActive = (bool)(subscriptionDictionary.ValueForKey(new NSString("isActive")) as NSNumber),
                        Name = subscriptionDictionary.ValueForKey(new NSString("name")) as NSString,
                        UserId = subscriptionDictionary.ValueForKey(new NSString("author")) as NSString,
                        SubscribedDate = FireTimeToDateTime(subscriptionDictionary.ValueForKey(new NSString("subscribedDate")) as Firebase.CloudFirestore.Timestamp),
                        Id = doc.Id
                    };

                    subscriptions.Add(subscription);
                }

                return subscriptions;
            }
            catch (Exception exc) 
            {
                return new List<Subscription>();
            }
        }

        private static NSDate DateTimeToNSDate(DateTime date) 
        {
            if (date.Kind == DateTimeKind.Unspecified)
                date = DateTime.SpecifyKind(date, DateTimeKind.Local);
            return (NSDate)date;
        }
        private static DateTime FireTimeToDateTime(Firebase.CloudFirestore.Timestamp date) 
        {
            DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0));
            return reference.AddSeconds(date.Seconds);
        }

        public async Task<bool> DeleteSubscription(Subscription subscription)
        {
            try
            {
                var collection = Firebase.CloudFirestore.Firestore.SharedInstance.GetCollection("subscriptions");
                await collection.GetDocument(subscription.Id).DeleteDocumentAsync();
                return true;
            }
            catch (Exception exc) 
            {
                return false;
            }
        }

        public async Task<bool> UpdateSubscription(Subscription subscription)
        {
            try
            {
                var keys = new[]
                {
                    new NSString("name"),
                    new NSString("isActive")
                };

                var values = new NSObject[]
                {
                    new NSString(subscription.Name),
                    new NSNumber(subscription.IsActive)
                };

                var subscriptionDocument = new NSDictionary<NSObject, NSObject>(keys, values);

                var collection = Firebase.CloudFirestore.Firestore.SharedInstance.GetCollection("subscriptions");
                await collection.GetDocument(subscription.Id).UpdateDataAsync(subscriptionDocument);

                return true;
            }
            catch (Exception exc) 
            { 
                return false;
            }
        }
    }
}