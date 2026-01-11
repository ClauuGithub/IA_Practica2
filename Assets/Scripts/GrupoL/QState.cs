using System;
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

namespace GrupoL
{
    public sealed class QState
    {
        private readonly int dxPlayer;
        private readonly int dyPlayer;
        private readonly int dangerLevel;

        private readonly bool wallUp;
        private readonly bool wallDown;
        private readonly bool wallLeft;
        private readonly bool wallRight;

        public QState(CellInfo agent, CellInfo other, WorldInfo world)
        {
            dxPlayer = Math.Sign (other.x - agent.x);
            dyPlayer = Math.Sign(other.y - agent.y);

            int manhattan=Math.Abs(other.x - agent.x)+ Math.Abs(other.y - agent.y);
            if (manhattan <= 1) dangerLevel = 0; //0= muy cerca
            else if (manhattan <= 3) dangerLevel = 1;// 1=cerca
            else dangerLevel = 2;// 2=lejos

            // Comprobar muros alrededor
            wallUp = IsWall(agent.x, agent.y + 1, world);
            wallDown = IsWall(agent.x, agent.y - 1, world);
            wallLeft = IsWall(agent.x - 1, agent.y, world);
            wallRight = IsWall(agent.x + 1, agent.y, world);
        }

        private bool IsWall(int x, int y, WorldInfo world)
        {
            // Fuera del grid = muro
            if (x < 0 || y < 0 || x >= world.WorldSize.x || y >= world.WorldSize.y)
                return true;

            return world[x, y].Type == CellInfo.CellType.Wall;
        }

        public string ToKey()
        {
            // Codificamos en un string simple: dx,dy + muros 1/0
            return $"{dxPlayer},{dyPlayer}|{dangerLevel}|{(wallUp ? 1 : 0)}{(wallDown ? 1 : 0)}{(wallLeft ? 1 : 0)}{(wallRight ? 1 : 0)}";
        }
    }


}