﻿using System;

namespace TwistedLogik.Ultraviolet.UI.Presentation.Uvss.Syntax
{
    /// <summary>
    /// Represents a visual transition.
    /// </summary>
    public sealed class UvssTransitionSyntax : UvssNodeSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UvssTransitionSyntax"/> class.
        /// </summary>
        public UvssTransitionSyntax(
            SyntaxToken transitionKeyword,
            UvssTransitionArgumentListSyntax argumentList,
            SyntaxToken colonToken,
            SyntaxToken storyboardNameToken,
            SyntaxToken qualifierToken,
            SyntaxToken semiColonToken)
            : base(SyntaxKind.Transition)
        {
            this.TransitionKeyword = transitionKeyword;
            this.ArgumentList = argumentList;
            this.ColonToken = colonToken;
            this.StoryboardNameToken = storyboardNameToken;
            this.QualifierToken = qualifierToken;
            this.SemiColonToken = semiColonToken;

            SlotCount = 6;
        }

        /// <inheritdoc/>
        public override SyntaxNode GetSlot(Int32 index)
        {
            switch (index)
            {
                case 0: return TransitionKeyword;
                case 1: return ArgumentList;
                case 2: return ColonToken;
                case 3: return StoryboardNameToken;
                case 4: return QualifierToken;
                case 5: return SemiColonToken;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// The transition's "transition" keyword.
        /// </summary>
        public SyntaxToken TransitionKeyword;

        /// <summary>
        /// The transition's argument list.
        /// </summary>
        public UvssTransitionArgumentListSyntax ArgumentList;

        /// <summary>
        /// The colon that separates the transition declaration from its value.
        /// </summary>
        public SyntaxToken ColonToken;

        /// <summary>
        /// The storyboard name that is associated with the transition.
        /// </summary>
        public SyntaxToken StoryboardNameToken;

        /// <summary>
        /// The transition's qualifier token.
        /// </summary>
        public SyntaxToken QualifierToken;

        /// <summary>
        /// The semi-colon that terminates the transition.
        /// </summary>
        public SyntaxToken SemiColonToken;
    }
}
