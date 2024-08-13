#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.UI.Composition;

public partial class BooleanKeyFrameAnimation : KeyFrameAnimation
{
	private readonly SortedDictionary<float, bool> _keyFrames = new();

	internal BooleanKeyFrameAnimation(Compositor compositor) : base(compositor)
	{
	}

	private protected override int KeyFrameCountCore => _keyFrames.Count;

	public void InsertKeyFrame(float normalizedProgressKey, bool value)
		=> _keyFrames[normalizedProgressKey] = value;

	internal override object? Start(ReadOnlySpan<char> propertyName, ReadOnlySpan<char> subPropertyName, CompositionObject compositionObject)
	{
		base.Start(propertyName, subPropertyName, compositionObject);
		if (!_keyFrames.TryGetValue(0, out var startValue))
		{
			startValue = (bool)compositionObject.GetAnimatableProperty(propertyName.ToString(), subPropertyName.ToString());
		}

		if (!_keyFrames.TryGetValue(1.0f, out var finalValue))
		{
			finalValue = _keyFrames.Values.LastOrDefault(startValue);
		}

		Func<bool, bool, float, bool> lerp = (value1, value2, _) => value1;
		_keyframeEvaluator = new KeyFrameEvaluator<bool>(startValue, finalValue, Duration, _keyFrames, lerp, IterationCount, IterationBehavior, Compositor);
		return startValue;
	}
}
