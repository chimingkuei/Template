﻿#pragma checksum "..\..\BuildingBlock.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "1EE9563FB30F8278B0C62BD38FF114D21194F879B8F5E0C27FE6BDB99F289D8A"
//------------------------------------------------------------------------------
// <auto-generated>
//     這段程式碼是由工具產生的。
//     執行階段版本:4.0.30319.42000
//
//     對這個檔案所做的變更可能會造成錯誤的行為，而且如果重新產生程式碼，
//     變更將會遺失。
// </auto-generated>
//------------------------------------------------------------------------------

using InnovaControls;
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


namespace InnovaControls {
    
    
    /// <summary>
    /// BuildingBlock
    /// </summary>
    public partial class BuildingBlock : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\BuildingBlock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas Canvas1;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\BuildingBlock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button DraggableButton;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\BuildingBlock.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas Canvas2;
        
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
            System.Uri resourceLocater = new System.Uri("/InnovaControls;component/buildingblock.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\BuildingBlock.xaml"
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
            this.Canvas1 = ((System.Windows.Controls.Canvas)(target));
            
            #line 10 "..\..\BuildingBlock.xaml"
            this.Canvas1.DragEnter += new System.Windows.DragEventHandler(this.Canvas1_DragEnter);
            
            #line default
            #line hidden
            
            #line 10 "..\..\BuildingBlock.xaml"
            this.Canvas1.Drop += new System.Windows.DragEventHandler(this.Canvas1_Drop);
            
            #line default
            #line hidden
            return;
            case 2:
            this.DraggableButton = ((System.Windows.Controls.Button)(target));
            
            #line 11 "..\..\BuildingBlock.xaml"
            this.DraggableButton.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.DraggableButton_PreviewMouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Canvas2 = ((System.Windows.Controls.Canvas)(target));
            
            #line 13 "..\..\BuildingBlock.xaml"
            this.Canvas2.DragEnter += new System.Windows.DragEventHandler(this.Canvas2_DragEnter);
            
            #line default
            #line hidden
            
            #line 13 "..\..\BuildingBlock.xaml"
            this.Canvas2.Drop += new System.Windows.DragEventHandler(this.Canvas2_Drop);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
