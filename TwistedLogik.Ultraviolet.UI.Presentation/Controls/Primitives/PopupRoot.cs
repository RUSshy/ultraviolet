﻿using System;

namespace TwistedLogik.Ultraviolet.UI.Presentation.Controls.Primitives
{
    /// <summary>
    /// Represents the root visual for elements inside of a <see cref="Popup"/> control.
    /// </summary>
    internal class PopupRoot : FrameworkElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PopupRoot"/> class.
        /// </summary>
        /// <param name="uv">The Ultraviolet context.</param>
        /// <param name="resized">The action to perform when the popup content is resized.</param>
        public PopupRoot(UltravioletContext uv, Action resized)
            : base(uv, null)
        {
            this.resized = resized;
        }

        /// <summary>
        /// Updates the popup root's transformation state.
        /// </summary>
        /// <param name="placementTarget">The popup's placement target, if it has one.</param>
        public void UpdateTransform(UIElement placementTarget)
        {
            inheritedRenderTransform = (placementTarget == null) ? Matrix.Identity :
                placementTarget.GetTransformToAncestorMatrix(placementTarget.View.LayoutRoot);
        }

        /// <summary>
        /// Gets or sets the popup root's child element.
        /// </summary>
        public UIElement Child
        {
            get { return GetValue<UIElement>(ChildProperty); }
            set { SetValue<UIElement>(ChildProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the popup is currently open.
        /// </summary>
        public Boolean IsOpen
        {
            get { return isOpen; }
            set 
            {
                if (isOpen != value)
                {
                    isOpen = value;
                    UpdateOutOfBandRegistration();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the popup is being transformed.
        /// </summary>
        public Boolean IsTransformed
        {
            get { return isTransformed; }
            set 
            {
                if (isTransformed != value)
                {
                    isTransformed = value;
                    UpdateOutOfBandRegistration();
                }
            }
        }

        /// <summary>
        /// Gets the render transform which the popup inherits from its placement target.
        /// </summary>
        public Matrix InheritedRenderTransform
        {
            get { return inheritedRenderTransform; }
        }

        /// <summary>
        /// Identifies the <see cref="Child"/> dependency property.
        /// </summary>
        /// <remarks>The styling name of this dependency property is 'child'.</remarks>
        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register("Child", typeof(UIElement), typeof(PopupRoot),
            new PropertyMetadata<UIElement>(null, PropertyMetadataOptions.None, HandleChildChanged));

        /// <inheritdoc/>
        protected internal override Int32 VisualChildrenCount
        {
            get { return (Child != null ? 1 : 0) + base.VisualChildrenCount; }
        }

        /// <inheritdoc/>
        protected internal override Int32 LogicalChildrenCount
        {
            get { return base.LogicalChildrenCount; }
        }

        /// <inheritdoc/>
        protected internal override UIElement GetVisualChild(Int32 childIndex)
        {
            var child = Child;
            if (child != null)
            {
                if (childIndex == 0)
                {
                    return child;
                }
                return base.GetVisualChild(childIndex - 1);
            }
            return base.GetVisualChild(childIndex);
        }

        /// <inheritdoc/>
        protected internal override UIElement GetLogicalChild(Int32 childIndex)
        {
            return base.GetLogicalChild(childIndex);
        }

        /// <inheritdoc/>
        protected override Size2D MeasureOverride(Size2D availableSize)
        {
            var child = Child;
            if (child != null)
            {
                child.Measure(availableSize);
                return child.DesiredSize;
            }
            return Size2D.Zero;
        }

        /// <inheritdoc/>
        protected override Size2D ArrangeOverride(Size2D finalSize, ArrangeOptions options)
        {
            var child = Child;
            if (child != null)
            {
                child.Arrange(new RectangleD(Point2D.Zero, finalSize), options);
                return child.RenderSize;
            }
            return base.ArrangeOverride(finalSize, options);
        }

        /// <inheritdoc/>
        protected override Visual HitTestCore(Point2D point)
        {
            var child = Child;
            if (child == null)
                return null;

            return child.HitTest(new Point2D(point.X - child.RelativeBounds.X, point.Y - child.RelativeBounds.Y));
        }

        /// <inheritdoc/>
        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            var popup = Parent as Popup;
            if (popup != null && popup.IsOpen)
            {
                if (resized != null)
                {
                    resized();
                }
            }
            base.OnChildDesiredSizeChanged(child);
        }

        /// <summary>
        /// Occurs when the value of the <see cref="Child"/> dependency property changes.
        /// </summary>
        private static void HandleChildChanged(DependencyObject dobj, UIElement oldValue, UIElement newValue)
        {
            var popupRoot = (PopupRoot)dobj;
            var popup     = popupRoot.Parent as Popup;

            if (oldValue != null)
                oldValue.ChangeVisualParent(null);

            if (popup.IsOpen)
            {
                newValue.ChangeVisualParent(popupRoot);
            }
            else
            {
                newValue.ChangeVisualParent(null);
            }

            popupRoot.UpdateOutOfBandRegistration();
        }

        /// <summary>
        /// Updates the out-of-band registration status of this popup's child element.
        /// </summary>
        private void UpdateOutOfBandRegistration()
        {
            var child = Child;
            if (child != null)
            {
                var upf = Ultraviolet.GetUI().GetPresentationFoundation();
                if (isTransformed && isOpen)
                {
                    upf.OutOfBandRenderer.Register(child);
                }
                else
                {
                    upf.OutOfBandRenderer.Unregister(child);
                }
            }
        }

        // State values.
        private readonly Action resized;
        private Boolean isOpen;
        private Boolean isTransformed;
        private Matrix inheritedRenderTransform;
    }
}
