#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueSharp;
using RogueSharp.Random;

#endregion

namespace ExampleGame
{
   /// <summary>
   /// This is the main type for your game
   /// </summary>
   public class Game1 : Game
   {
      GraphicsDeviceManager graphics;
      SpriteBatch spriteBatch;

      private Texture2D _floor;
      private Texture2D _wall;
      private IMap _map;
      private Player _player;
      private AggressiveEnemy _aggressiveEnemy;
      private CowardlyEnemy _cowardlyEnemy;
      private InputState _inputState;

      public Game1()
         : base()
      {
         graphics = new GraphicsDeviceManager( this );
         Content.RootDirectory = "Content";
         _inputState = new InputState();
      }

      /// <summary>
      /// Allows the game to perform any initialization it needs to before starting to run.
      /// This is where it can query for any required services and load any non-graphic
      /// related content.  Calling base.Initialize will enumerate through any components
      /// and initialize them as well.
      /// </summary>
      protected override void Initialize()
      {
         // TODO: Add your initialization logic here
         IMapCreationStrategy<Map> mapCreationStrategy = new RandomRoomsMapCreationStrategy<Map>( 50, 30, 100, 7, 3 );
         _map = Map.Create( mapCreationStrategy );

         base.Initialize();
      }

      /// <summary>
      /// LoadContent will be called once per game and is the place to load
      /// all of your content.
      /// </summary>
      protected override void LoadContent()
      {
         // Create a new SpriteBatch, which can be used to draw textures.
         spriteBatch = new SpriteBatch( GraphicsDevice );

         // TODO: use this.Content to load your game content here
         _floor = Content.Load<Texture2D>( "Floor" );
         _wall = Content.Load<Texture2D>( "Wall" );
         Cell startingCell = GetRandomEmptyCell();
         _player = new Player
         {
            X = startingCell.X,
            Y = startingCell.Y,
            Scale = 0.25f,
            Sprite = Content.Load<Texture2D>( "Player" )
         };
         UpdatePlayerFieldOfView();
         startingCell = GetRandomEmptyCell();
         var pathFromAggressiveEnemy = new PathToPlayer( _player, _map, Content.Load<Texture2D>( "White" ) );
         pathFromAggressiveEnemy.CreateFrom( startingCell.X, startingCell.Y ); 
         _aggressiveEnemy = new AggressiveEnemy( pathFromAggressiveEnemy )
         {
            X = startingCell.X,
            Y = startingCell.Y,
            Scale = 0.25f,
            Sprite = Content.Load<Texture2D>( "Hound" )
         };
         startingCell = GetRandomEmptyCell();
         var pathAvoidingPlayer = new PathAvoidingPlayer( _player, _map, Content.Load<Texture2D>( "White" ) );
         pathAvoidingPlayer.CreateFrom( startingCell.X, startingCell.Y );
         _cowardlyEnemy = new CowardlyEnemy( pathAvoidingPlayer )
         {
            X = startingCell.X,
            Y = startingCell.Y,
            Scale = 0.25f,
            Sprite = Content.Load<Texture2D>( "Hound" )
         };
         Global.GameState = GameStates.PlayerTurn;
      }

      /// <summary>
      /// UnloadContent will be called once per game and is the place to unload
      /// all content.
      /// </summary>
      protected override void UnloadContent()
      {
         // TODO: Unload any non ContentManager content here
      }

      /// <summary>
      /// Allows the game to run logic such as updating the world,
      /// checking for collisions, gathering input, and playing audio.
      /// </summary>
      /// <param name="gameTime">Provides a snapshot of timing values.</param>
      protected override void Update( GameTime gameTime )
      {
         // TODO: Add your update logic here
         _inputState.Update();
         if ( _inputState.IsExitGame( PlayerIndex.One ) )
         {
            Exit();
         }
         else if ( _inputState.IsSpace( PlayerIndex.One ) )
         {
            if ( Global.GameState == GameStates.PlayerTurn )
            {
               Global.GameState = GameStates.Debugging;
            }
            else if ( Global.GameState == GameStates.Debugging )
            {
               Global.GameState = GameStates.PlayerTurn;
            }
         }
         else
         {
            if ( Global.GameState == GameStates.PlayerTurn 
               && _player.HandleInput( _inputState, _map ) )
            {
               UpdatePlayerFieldOfView();
               if ( _map.IsInFov(_cowardlyEnemy.X, _cowardlyEnemy.Y ) )
               {
                  _cowardlyEnemy.HasBeenFound = true;
               }
               Global.GameState = GameStates.EnemyTurn;
            }
            if ( Global.GameState == GameStates.EnemyTurn )
            {
               _aggressiveEnemy.Update();
               _cowardlyEnemy.Update();
               Global.GameState = GameStates.PlayerTurn;
            }
         }

         base.Update( gameTime );
      }

      /// <summary>
      /// This is called when the game should draw itself.
      /// </summary>
      /// <param name="gameTime">Provides a snapshot of timing values.</param>
      protected override void Draw( GameTime gameTime )
      {
         GraphicsDevice.Clear( Color.Black );

         // TODO: Add your drawing code here
         spriteBatch.Begin( SpriteSortMode.BackToFront, BlendState.AlphaBlend );

         int sizeOfSprites = 64;
         float scale = .25f;
         foreach ( Cell cell in _map.GetAllCells() )
         {
            var position = new Vector2( cell.X * sizeOfSprites * scale, cell.Y * sizeOfSprites * scale );
            if ( !cell.IsExplored && Global.GameState != GameStates.Debugging )
            {
               continue;
            }
            Color tint = Color.White;
            if ( !cell.IsInFov && Global.GameState != GameStates.Debugging )
            {
               tint = Color.Gray;
            }
            if ( cell.IsWalkable )
            {
               spriteBatch.Draw( _floor, position, null, null, null, 0.0f, new Vector2( scale, scale ), tint, SpriteEffects.None, 0.8f );
            }
            else
            {
               spriteBatch.Draw( _wall, position, null, null, null, 0.0f, new Vector2( scale, scale ), tint, SpriteEffects.None, 0.8f );
            }
         }

         _player.Draw( spriteBatch );
         if ( Global.GameState == GameStates.Debugging || _map.IsInFov( _aggressiveEnemy.X, _aggressiveEnemy.Y ) )
         {
            _aggressiveEnemy.Draw( spriteBatch );
         }
         if ( Global.GameState == GameStates.Debugging || _map.IsInFov( _cowardlyEnemy.X, _cowardlyEnemy.Y ) )
         {
            _cowardlyEnemy.Draw( spriteBatch );
         }

         spriteBatch.End();

         base.Draw( gameTime );
      }

      private void UpdatePlayerFieldOfView()
      {
         _map.ComputeFov( _player.X, _player.Y, 30, true );
         foreach ( Cell cell in _map.GetAllCells() )
         {
            if ( _map.IsInFov( cell.X, cell.Y ) )
            {
               _map.SetCellProperties( cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable, true );
            }
         }
      }

      private Cell GetRandomEmptyCell()
      {
         while ( true )
         {
            int x = Global.Random.Next( 49 );
            int y = Global.Random.Next( 29 );
            if ( _map.IsWalkable( x, y ) )
            {
               return _map.GetCell( x, y );
            }
         }
      }
   }
}
