using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueSharp;

namespace ExampleGame
{
   public class PathAvoidingPlayer
   {
      private readonly Player _player;
      private readonly IMap _map;
      private readonly Texture2D _sprite;
      private readonly GoalMap _goalMap;
      private IEnumerable<RogueSharp.Point> _pathAwayFromPlayer;

      public PathAvoidingPlayer( Player player, IMap map, Texture2D sprite )
      {
         _player = player;
         _map = map;
         _sprite = sprite;
         _goalMap = new GoalMap( map );
      }
      public Cell FirstCell
      {
         get
         {
            if ( _pathAwayFromPlayer.Count() > 1 )
            {
               RogueSharp.Point secondPoint = _pathAwayFromPlayer.Skip( 1 ).First();
               return _map.GetCell( secondPoint.X, secondPoint.Y );
            }
            RogueSharp.Point point = _pathAwayFromPlayer.First();
            return _map.GetCell( point.X, point.Y );
         }
      }
      public void CreateFrom( int x, int y )
      {
         _goalMap.ClearGoals();
         _goalMap.AddGoal( _player.X, _player.Y, 0 );
         _pathAwayFromPlayer = _goalMap.FindPathAvoidingGoals( x, y, new List<RogueSharp.Point>() );
      }
      public void Draw( SpriteBatch spriteBatch )
      {
         if ( _pathAwayFromPlayer != null && Global.GameState == GameStates.Debugging )
         {
            foreach ( RogueSharp.Point point in _pathAwayFromPlayer )
            {
               float scale = .25f;
               float multiplier = .25f * _sprite.Width;
               spriteBatch.Draw( _sprite, new Vector2( point.X * multiplier, point.Y * multiplier ), null, null, null, 0.0f, new Vector2( scale, scale ), Color.Blue * .2f, SpriteEffects.None, 0.6f );
            }
         }
      }
   }
}
