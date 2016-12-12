using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace HoloToolkit.Sharing
{
    public class ImageTagLocationAdapter : ImageTagLocationListener
    {
        public event Action<ImageTagLocation> LocatedEvent;
        public event Action CompletedEvent;

        public override void OnTagLocated(ImageTagLocation location)
        {
            if (this.LocatedEvent != null)
            {
                this.LocatedEvent(location);
            }
        }

        public override void OnTagLocatingCompleted()
        {
            if (this.CompletedEvent != null)
            {
                this.CompletedEvent();
            }
        }
    }
}