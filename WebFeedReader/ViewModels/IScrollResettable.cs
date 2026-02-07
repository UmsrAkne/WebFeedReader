using System;

namespace WebFeedReader.ViewModels
{
    public interface IScrollResettable
    {
        event Action RequestScrollReset;
    }
}