﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TwistedLogik.Nucleus;

namespace TwistedLogik.Ultraviolet.UI.Presentation
{
    /// <summary>
    /// Contains methods for generating invocation delegates for routed events.
    /// </summary>
    internal static class RoutedEventInvocation
    {
        /// <summary>
        /// Initializes the <see cref="RoutedEventInvocation"/> type.
        /// </summary>
        static RoutedEventInvocation()
        {
            miShouldEventBeRaisedForElement = typeof(RoutedEventInvocation).GetMethod("ShouldEventBeRaisedForElement", BindingFlags.NonPublic | BindingFlags.Static);
            miShouldContinueBubbling        = typeof(RoutedEventInvocation).GetMethod("ShouldContinueBubbling", BindingFlags.NonPublic | BindingFlags.Static);
            miShouldContinueTunnelling      = typeof(RoutedEventInvocation).GetMethod("ShouldContinueTunnelling", BindingFlags.NonPublic | BindingFlags.Static);
            miGetEventHandler               = typeof(RoutedEventInvocation).GetMethod("GetEventHandler", BindingFlags.NonPublic | BindingFlags.Static);
        }

        /// <summary>
        /// Creates an invocation delegate for the specified routed event.
        /// </summary>
        /// <param name="evt">A <see cref="RoutedEvent"/> which identifies the routed event for which to create an invocation delegate.</param>
        /// <returns>The invocation delegate for the specified routed event.</returns>
        public static Delegate CreateInvocationDelegate(RoutedEvent evt)
        {
            Contract.Require(evt, "evt");

            if (!IsValidRoutedEventDelegate(evt.DelegateType))
                throw new InvalidOperationException(PresentationStrings.InvalidRoutedEventDelegate.Format(evt.DelegateType.Name));

            switch (evt.RoutingStrategy)
            {
                case RoutingStrategy.Bubble:
                    return CreateInvocationDelegateForBubbleStrategy(evt);
                
                case RoutingStrategy.Direct:
                    return CreateInvocationDelegateForDirectStrategy(evt);

                case RoutingStrategy.Tunnel:
                    return CreateInvocationDelegateForTunnelStrategy(evt);

                default:
                    throw new InvalidOperationException(PresentationStrings.InvalidRoutingStrategy);
            }
        }

        /// <summary>
        /// Creates an invocation delegate for a routed event which uses the <see cref="RoutingStrategy.Bubble"/> routing strategy.
        /// </summary>
        /// <param name="evt">A <see cref="RoutedEvent"/> which identifies the routed event for which to create an invocation delegate.</param>
        /// <returns>The invocation delegate for the specified routed event.</returns>
        private static Delegate CreateInvocationDelegateForBubbleStrategy(RoutedEvent evt)
        {
            /* BUBBLE STRATEGY
             * For a given event delegate type TDelegate, we're constructing a method which basically looks like this:
             * 
             * void fn(UIElement element, p1, p2, ..., pN, ref Boolean handled)
             * {
             *      handled = false;
             *      
             *      var index   = 0;
             *      var handler = default(RoutedEventHandlerMetadata);
             *      var current = element;
             *      
             *      while (ShouldContinueBubbling(element, ref current))
             *      {
             *          index = 0;
             *          while (GetEventHandler(current, RoutedEventID, index, out handler))
             *          {
             *              if (ShouldEventBeRaisedForElement(current, handled, handler.HandledEventsToo))
             *              {
             *                  handler.Handler(current, p1, p2, ..., pN, ref handled);
             *              }
             *          }
             *      }
             * }
             */

            var evtInvoke = evt.DelegateType.GetMethod("Invoke");
            var evtParams = evtInvoke.GetParameters().ToArray();

            var expParams       = evtParams.Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToList();
            var expParamElement = expParams.First();
            var expParamHandled = expParams.Last();

            var expParts = new List<Expression>();
            var expVars  = new List<ParameterExpression>();

            var expAssignedHandledToFalse = Expression.Assign(expParamHandled, Expression.Constant(false));
            expParts.Add(expAssignedHandledToFalse);

            var varIndex = Expression.Variable(typeof(Int32), "index");
            expVars.Add(varIndex);

            var varHandler = Expression.Variable(typeof(RoutedEventHandlerMetadata), "handlers");
            expVars.Add(varHandler);

            var varCurrent = Expression.Variable(typeof(UIElement), "current");
            expVars.Add(varCurrent);

            var innerEventHandlerParams = new List<ParameterExpression>();
            innerEventHandlerParams.Add(varCurrent);
            innerEventHandlerParams.AddRange(expParams.Skip(1));

            var expWhileBubbleBreakOuter = Expression.Label();
            var expWhileBubbleBreakInner = Expression.Label();

            var expWhileBubble = Expression.Loop(
                Expression.IfThenElse(
                    Expression.Call(miShouldContinueBubbling, expParamElement, varCurrent),
                    Expression.Block(
                        Expression.Assign(varIndex, Expression.Constant(0)),
                        Expression.Loop(
                            Expression.IfThenElse(
                                Expression.Call(miGetEventHandler, varCurrent, Expression.Constant(evt), varIndex, varHandler),
                                Expression.IfThen(
                                    Expression.Call(miShouldEventBeRaisedForElement, expParamHandled, Expression.Property(varHandler, "HandledEventsToo")),
                                    Expression.Invoke(Expression.Convert(Expression.Property(varHandler, "Handler"), evt.DelegateType), innerEventHandlerParams)
                                ),
                                Expression.Break(expWhileBubbleBreakInner)
                            ),
                            expWhileBubbleBreakInner
                        )
                    ),
                    Expression.Break(expWhileBubbleBreakOuter)
                ),
                expWhileBubbleBreakOuter
            );
            expParts.Add(expWhileBubble);

            return Expression.Lambda(evt.DelegateType, Expression.Block(expVars, expParts), expParams).Compile();
        }

        /// <summary>
        /// Creates an invocation delegate for a routed event which uses the <see cref="RoutingStrategy.Direct"/> routing strategy.
        /// </summary>
        /// <param name="evt">A <see cref="RoutedEvent"/> which identifies the routed event for which to create an invocation delegate.</param>
        /// <returns>The invocation delegate for the specified routed event.</returns>
        private static Delegate CreateInvocationDelegateForDirectStrategy(RoutedEvent evt)
        {
            /* DIRECT STRATEGY
             * The simplest strategy; the event is only invoked on the element that raised it. 
             * Our invocation delegate looks something like this:
             * 
             * void fn(UIElement element, p1, p2, ..., pN, ref Boolean handled)
             * {
             *      handled = false;
             * 
             *      var index   = 0;
             *      var handler = default(RoutedEventHandlerMetadata);      
             * 
             *      while (GetEventHandler(current, RoutedEventID, index, out handler))
             *      {
             *          if (ShouldEventBeRaisedForElement(element, handled, handlerInfo.HandledEventsToo))
             *          {
             *              handler.Handler(element, p1, p2, ..., pN, ref handled);
             *          }
             *      }
             * }
             */

            var evtInvoke = evt.DelegateType.GetMethod("Invoke");
            var evtParams = evtInvoke.GetParameters().ToArray();

            var expParams       = evtParams.Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToList();
            var expParamElement = expParams.First();
            var expParamHandled = expParams.Last();

            var expParts = new List<Expression>();
            var expVars  = new List<ParameterExpression>();

            var expAssignedHandledToFalse = Expression.Assign(expParamHandled, Expression.Constant(false));
            expParts.Add(expAssignedHandledToFalse);

            var varIndex = Expression.Variable(typeof(Int32), "index");
            expVars.Add(varIndex);

            var varHandler = Expression.Variable(typeof(RoutedEventHandlerMetadata), "handlers");
            expVars.Add(varHandler);

            var expInvokeBreak = Expression.Label();

            var expInvoke = Expression.Loop(
                Expression.IfThenElse(
                    Expression.Call(miGetEventHandler, expParamElement, Expression.Constant(evt), varIndex, varHandler),
                    Expression.IfThen(
                        Expression.Call(miShouldEventBeRaisedForElement, expParamHandled, Expression.Property(varHandler, "HandledEventsToo")),
                        Expression.Invoke(Expression.Convert(Expression.Property(varHandler, "Handler"), evt.DelegateType), expParams)
                    ),
                    Expression.Break(expInvokeBreak)
                ),
                expInvokeBreak
            );
            expParts.Add(expInvoke);

            return Expression.Lambda(evt.DelegateType, Expression.Block(expVars, expParts), expParams).Compile();
        }

        /// <summary>
        /// Creates an invocation delegate for a routed event which uses the <see cref="RoutingStrategy.Tunnel"/> routing strategy.
        /// </summary>
        /// <param name="evt">A <see cref="RoutedEvent"/> which identifies the routed event for which to create an invocation delegate.</param>
        /// <returns>The invocation delegate for the specified routed event.</returns>
        private static Delegate CreateInvocationDelegateForTunnelStrategy(RoutedEvent evt)
        {
            /* TUNNEL STRATEGY
             * Basically the opposite of the bubble strategy; we start at the root of the tree and work down.
             * Note that ShouldContinueTunnelling() builds a stack representing the path to take on the first call.
             * 
             * void fn(UIElement element, p1, p2, ..., pN, ref Boolean handled)
             * {
             *      handled = false;
             * 
             *      var index    = 0;
             *      var current  = default(UIElement);
             *      var handlers = default(List<RoutedEventHandlerMetadata>);
             *      
             *      while (ShouldContinueTunnelling(element, ref current))
             *      {
             *          index = 0;
             *          while (GetEventHandler(current, RoutedEventID, index, out handler))
             *          {
             *              if (ShouldEventBeRaisedForElement(current, handled, handler.HandledEventsToo))
             *              {
             *                  handler.Handler(current, p1, p2, ..., pN, ref handled);
             *              }
             *          }
             *      }
             * }
             */

            var evtInvoke = evt.DelegateType.GetMethod("Invoke");
            var evtParams = evtInvoke.GetParameters().ToArray();

            var expParams       = evtParams.Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToList();
            var expParamElement = expParams.First();
            var expParamHandled = expParams.Last();

            var expParts = new List<Expression>();
            var expVars  = new List<ParameterExpression>();

            var expAssignedHandledToFalse = Expression.Assign(expParamHandled, Expression.Constant(false));
            expParts.Add(expAssignedHandledToFalse);

            var varIndex = Expression.Variable(typeof(Int32), "index");
            expVars.Add(varIndex);

            var varHandler = Expression.Variable(typeof(RoutedEventHandlerMetadata), "handler");
            expVars.Add(varHandler);

            var varCurrent = Expression.Variable(typeof(UIElement), "current");
            expVars.Add(varCurrent);

            var innerEventHandlerParams = new List<ParameterExpression>();
            innerEventHandlerParams.Add(varCurrent);
            innerEventHandlerParams.AddRange(expParams.Skip(1));

            var expWhileTunnelBreakOuter = Expression.Label();
            var expWhileTunnelBreakInner = Expression.Label();

            var expWhileTunnel = Expression.Loop(
                Expression.IfThenElse(
                    Expression.Call(miShouldContinueTunnelling, expParamElement, varCurrent),
                    Expression.Block(
                        Expression.Assign(varIndex, Expression.Constant(0)),
                        Expression.Loop(
                            Expression.IfThenElse(
                                Expression.Call(miGetEventHandler, varCurrent, Expression.Constant(evt), varIndex, varHandler),
                                Expression.IfThen(
                                    Expression.Call(miShouldEventBeRaisedForElement, expParamHandled, Expression.Property(varHandler, "HandledEventsToo")),
                                    Expression.Invoke(Expression.Convert(Expression.Property(varHandler, "Handler"), evt.DelegateType), innerEventHandlerParams)
                                ),
                                Expression.Break(expWhileTunnelBreakInner)
                            ),
                            expWhileTunnelBreakInner
                        )
                    ),
                    Expression.Break(expWhileTunnelBreakOuter)
                ),
                expWhileTunnelBreakOuter
            );
            expParts.Add(expWhileTunnel);

            return Expression.Lambda(evt.DelegateType, Expression.Block(expVars, expParts), expParams).Compile();
        }

        /// <summary>
        /// Gets a value indicating whether the specified type represents a valid routed event delegate.
        /// </summary>
        /// <param name="delegateType">The delegate type to evaluate.</param>
        /// <returns><c>true</c> if the specified delegate type represents a valid routed event delegate; otherwise, <c>false</c>.</returns>
        private static Boolean IsValidRoutedEventDelegate(Type delegateType)
        {
            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                return false;

            var invoke     = delegateType.GetMethod("Invoke");
            var parameters = invoke.GetParameters().ToList();

            if (parameters.Count < 2)
                return false;

            var paramFirst = parameters.First();
            var paramLast  = parameters.Last();

            return
                typeof(UIElement).IsAssignableFrom(paramFirst.ParameterType) &&
                paramLast.ParameterType == typeof(Boolean).MakeByRefType() &&
                invoke.ReturnType == typeof(void);
        }

        /// <summary>
        /// Gets a value indicating whether the specified element should receive the event being processed.
        /// </summary>
        /// <param name="handled">A value indicating whether the event has been handled.</param>
        /// <param name="handledEventsToo">A value indicating whether the current event handler wants to receive handled events.</param>
        /// <returns><c>true</c> if the event should be raised for this object; otherwise, <c>false</c>.</returns>
        private static Boolean ShouldEventBeRaisedForElement(Boolean handled, Boolean handledEventsToo)
        {
            return !handled || handledEventsToo;
        }

        /// <summary>
        /// Gets a value indicating whether the event being processed should continue bubbling up the 
        /// element hierarchy, and if so, sets the <paramref name="current"/> parameter to
        /// the next object to be processed.
        /// </summary>
        /// <param name="first">The first element to process.</param>
        /// <param name="current">The element that is currently being processed.</param>
        /// <returns><c>true</c> if the event should continue bubbling; otherwise, <c>false</c>.</returns>
        private static Boolean ShouldContinueBubbling(UIElement first, ref UIElement current)
        {
            if (current == null)
            {
                current = first;
                return true;
            }
            else
            {
                if (current.DependencyContainer == null)
                {
                    current = null;
                    return false;
                }
                current = current.DependencyContainer as UIElement;
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the event being processed should continue tunnelling down the 
        /// element hierarchy, and if so, sets the <paramref name="current"/> parameter to
        /// the next object to be processed.
        /// </summary>
        /// <param name="first">The first element to process.</param>
        /// <param name="current">The element that is currently being processed.</param>
        /// <returns><c>true</c> if the event should continue tunnelling; otherwise, <c>false</c>.</returns>
        private static Boolean ShouldContinueTunnelling(UIElement first, ref UIElement current)
        {
            if (current == null)
                PrepareTunnellingStack(first);

            if (tunnellingStack.Count > 0)
            {
                var next = tunnellingStack.Pop();
                current = next;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Prepares the tunnelling stack to process a tunnelled event.
        /// </summary>
        /// <param name="element">The element which raised the event.</param>
        private static void PrepareTunnellingStack(UIElement element)
        {
            if (tunnellingStack == null)
                tunnellingStack = new Stack<UIElement>();

            tunnellingStack.Clear();

            for (var current = element; current != null; current = current.DependencyContainer as UIElement)
            {
                tunnellingStack.Push(current);
            }
        }

        /// <summary>
        /// Gets the next event handler to invoke for the current element.
        /// </summary>
        /// <param name="element">The element for which event handlers are being invoked.</param>
        /// <param name="evt">A <see cref="RoutedEvent"/> which identifies the routed event for which handlers are being invoked.</param>
        /// <param name="index">The index of the handler to invoke; this value is incremented by one when this method returns.</param>
        /// <param name="handler">The metadata for the handler that corresponds to the specified index within the handler list.</param>
        /// <returns><c>true</c> if a handler was retrieved for the specified index; otherwise, <c>false</c>.</returns>
        private static Boolean GetEventHandler(UIElement element, RoutedEvent evt, ref Int32 index, ref RoutedEventHandlerMetadata handler)
        {
            var handlers = element.GetHandlers(evt);
            if (handlers == null || index >= handlers.Count)
            {
                return false;
            }
            handler = handlers[index++];
            return true;
        }

        // Cached method info for methods used by invocation delegates.
        private static readonly MethodInfo miShouldEventBeRaisedForElement;
        private static readonly MethodInfo miShouldContinueBubbling;
        private static readonly MethodInfo miShouldContinueTunnelling;
        private static readonly MethodInfo miGetEventHandler;

        // The stack used to track the tunnelling path for tunnelled events.
        [ThreadStatic]
        private static Stack<UIElement> tunnellingStack;
    }
}
