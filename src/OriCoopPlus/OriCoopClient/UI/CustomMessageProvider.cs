using System.Collections.Generic;
using UnityEngine;

namespace MP_Client.UI
{
    public class CustomMessageProvider : MessageProvider
    {
        public string Text = "";

        public static CustomMessageProvider Create(string text)
        {
            var provider = ScriptableObject.CreateInstance<CustomMessageProvider>();
            provider.Text = text;
            return provider;
        }

        public override IEnumerable<MessageDescriptor> GetMessages()
        {
            return new MessageDescriptor[] { new MessageDescriptor(Text) };
        }
    }
}
