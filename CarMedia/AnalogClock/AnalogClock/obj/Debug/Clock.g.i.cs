﻿#pragma checksum "..\..\Clock.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "FDB0131794BF5BD9E50FB21BAD9E7877"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34011
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Expression.Controls;
using Microsoft.Expression.Media;
using Microsoft.Expression.Shapes;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace AnalogClock {
    
    
    /// <summary>
    /// Clock
    /// </summary>
    public partial class Clock : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 9 "..\..\Clock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal AnalogClock.Clock UserControl;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\Clock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid LayoutRoot;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\Clock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid ClockGrid;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\Clock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse Circle1;
        
        #line default
        #line hidden
        
        
        #line 784 "..\..\Clock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse CentreCircle;
        
        #line default
        #line hidden
        
        
        #line 785 "..\..\Clock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Microsoft.Expression.Shapes.RegularPolygon secondHand;
        
        #line default
        #line hidden
        
        
        #line 788 "..\..\Clock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.RotateTransform secondHandTransform;
        
        #line default
        #line hidden
        
        
        #line 793 "..\..\Clock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Microsoft.Expression.Shapes.RegularPolygon Minutehand;
        
        #line default
        #line hidden
        
        
        #line 802 "..\..\Clock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Microsoft.Expression.Shapes.RegularPolygon Hourhand;
        
        #line default
        #line hidden
        
        
        #line 803 "..\..\Clock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Ellipse CentreCircle_Copy;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/AnalogClock;component/clock.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\Clock.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.UserControl = ((AnalogClock.Clock)(target));
            return;
            case 2:
            this.LayoutRoot = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.ClockGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 4:
            this.Circle1 = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 5:
            this.CentreCircle = ((System.Windows.Shapes.Ellipse)(target));
            return;
            case 6:
            this.secondHand = ((Microsoft.Expression.Shapes.RegularPolygon)(target));
            return;
            case 7:
            this.secondHandTransform = ((System.Windows.Media.RotateTransform)(target));
            return;
            case 8:
            this.Minutehand = ((Microsoft.Expression.Shapes.RegularPolygon)(target));
            return;
            case 9:
            this.Hourhand = ((Microsoft.Expression.Shapes.RegularPolygon)(target));
            return;
            case 10:
            this.CentreCircle_Copy = ((System.Windows.Shapes.Ellipse)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

