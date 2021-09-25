﻿using boilersGraphics.ViewModels;
using Prism.Mvvm;

namespace boilersGraphics.Helpers
{
    public class TransformNotification : BindableBase
    {
        public DesignerItemViewModelBase Sender { get; set; }
        public string PropertyName { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
}
