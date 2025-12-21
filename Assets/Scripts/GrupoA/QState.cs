using System.Text;
using NavigationDJIA.World;
using NavigationDJIA.Interfaces;

/// <summary>
/// TODO(alumno):
/// Define el "estado" que usará la Tabla Q para identificar cada situación del agente.
/// 
/// El estado debe contener toda la información necesaria para que el agente pueda
/// tomar decisiones informadas. Tú decides qué características incluir según lo
/// que consideres relevante para resolver el problema.
/// 
/// Ejemplos típicos de información que puede formar un estado:
///   - Posición del agente en la grid.
///   - Posición del otro personaje (enemigo).
///   - Distancia relativa entre agente y enemigo.
///   - Si hay muros en direcciones cercanas.
///   - Cualquier otro dato que consideres útil.
/// 
/// En este ejercicio te damos un ejemplo simple basado únicamente en las posiciones
/// del agente y del oponente. Puedes usarlo tal cual o ampliarlo.
/// 
/// IMPORTANTE: 
///  El estado debe poder convertirse a una clave única (string) mediante ToKey(),
///  ya que esa clave se usará como índice en la TablaQ y en el archivo CSV.
/// </summary>

namespace GrupoA
{
    public sealed class QState 
    {
        private WorldInfo _world;

        // Ventana 5x5 = 25 celdas
        // 0 = libre, 1 = muro, 2 = zombie
        private readonly int[] localGrid;

        // Dirección relativa del zombie
        private readonly int zombieDx; // -1, 0, 1
        private readonly int zombieDy; // -1, 0, 1

        public QState(CellInfo agent, CellInfo other, WorldInfo world)
        {
            _world = world;

            localGrid = new int[25];

            int index = 0;

            bool IsInside(int x, int y)
            {
                return x >= 0 && y >= 0 &&
                       x < _world.WorldSize.x &&
                       y < _world.WorldSize.y;
            }

            bool IsWall(int x, int y)
            {
                /*if (!IsInside(x, y))
                    return true;*/

                return _world[x, y].Type == CellInfo.CellType.Wall;
            }

            for (int dy = 2; dy >= -2; dy--)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    int x = agent.x + dx;
                    int y = agent.y + dy;

                    if (!IsInside(x, y))
                    {
                        localGrid[index++] = 1; // fuera = muro
                    }
                    else if (IsWall(x, y))
                    {
                        localGrid[index++] = 1;
                    }
                    else if (x == other.x && y == other.y)
                    {
                        localGrid[index++] = 2;
                    }
                    else
                    {
                        localGrid[index++] = 0;
                    }
                }
            }

            zombieDx = Sign(other.x - agent.x);
            zombieDy = Sign(other.y - agent.y);
        }

        private int Sign(int value)
        {
            if (value > 0) return 1;
            if (value < 0) return -1;
            return 0;
        }

        public string ToKey()
        {
            StringBuilder sb = new StringBuilder(64);

            for (int i = 0; i < localGrid.Length; i++)
            {
                sb.Append(localGrid[i]);
            }

            sb.Append('|');
            sb.Append(zombieDx);
            sb.Append(',');
            sb.Append(zombieDy);

            return sb.ToString();
        }

        


    }
}