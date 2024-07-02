using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Chunks {

    public interface IChunksHelper {
        List<Vector2Int> GetChunksAround( int nGenerateDistance );
    }

    public class ChunksHelper : IChunksHelper {
        public List<Vector2Int> GetChunksAround( int nGenerateDistance ) {
            List<Vector2Int> aChunksAround = new List<Vector2Int>( );

            for (int x = -nGenerateDistance; x <= nGenerateDistance; x++) {
                for (int y = -nGenerateDistance; y <= nGenerateDistance; y++)
                    aChunksAround.Add( new Vector2Int( x, y ) );
            }

            return aChunksAround;
        }
    }
}