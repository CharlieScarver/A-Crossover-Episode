#region Using

using System;
using System.Numerics;
using Emotion.Engine;
using Emotion.Primitives;
using EmotionPlayground.GameObjects;

#endregion

namespace ACrossoverEpisode.Game.EventSystem
{
    /// <summary>
    /// An event which is triggered when a unit is a certain distance away from a point.
    /// </summary>
    public class DistanceEventListener : EventListener
    {
        private Unit _unit;
        private Vector2 _point;
        private float _distance;

        public DistanceEventListener(Unit unit, Vector3 point, float distance, Action action) : base(action)
        {
            _unit = unit;
            _point = new Vector2(point.X, point.Y);
            _distance = distance;
        }

        public override void Check()
        {
            Triggered = Vector2.Distance(_unit.Center, _point) >= _distance;
        }

        public override void DebugDraw()
        {
            Context.Renderer.RenderCircleOutline(new Vector3(_point, 0), _distance, Color.Red, true);
        }
    }
}