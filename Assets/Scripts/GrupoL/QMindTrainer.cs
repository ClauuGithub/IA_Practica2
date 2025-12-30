using System;
using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using System.Collections.Generic;

namespace GrupoL
{
    public class QMindTrainer : IQMindTrainer
    {
        private QMindTrainerParams _params;
        private WorldInfo _worldInfo;
        INavigationAlgorithm _navigationAlgorithm;

        private QTableStorage _qStorage;
        private QTable _qTable;

        private CellInfo _agentPosition;
        private CellInfo _otherPosition;

        private float _return;
        private float _returnAveraged;
        private System.Random _random = new System.Random();


        #region IQMindTrainer implementation

        public CellInfo AgentPosition => _agentPosition;
        public CellInfo OtherPosition => _otherPosition;

        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }

        public float Return => _return;
        public float ReturnAveraged => _returnAveraged;

        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        #endregion

        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _params = qMindTrainerParams;
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(worldInfo);

            _qStorage = new QTableStorage("TablaQ.csv");
            _qTable = new QTable(_qStorage);

            CurrentEpisode = 0;
            StartNewEpisode();
        }

        private void StartNewEpisode()
        {
            CurrentEpisode++;
            CurrentStep = 0;
            _return = 0f;
            _returnAveraged = 0f;

            _agentPosition = _worldInfo.RandomCell();
            _otherPosition = _worldInfo.RandomCell();

            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        private void EndEpisode()
        {
            _qTable.SaveToCsv();

            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);

            if (_params.episodes > 0 && CurrentEpisode >= _params.episodes)
            {
                return;
            }

            StartNewEpisode();
        }

        public void DoStep(bool train)
        {
            // Estado actual del agente
            string stateKey = BuildStateKey(_agentPosition, _otherPosition);

            // Seleciona la acción a realizar
            QAction action = ChooseAction(stateKey, train);

            // Nuevos estados del agente y del oponente
            CellInfo newAgentPos = ApplyAction(_agentPosition, action);
            CellInfo newOtherPos = MoveOpponent(_otherPosition, newAgentPos.Walkable ? newAgentPos : _agentPosition);
            
            // Nuevo estado del agente
            string nextStateKey = BuildStateKey(newAgentPos, newOtherPos);
            
            // Calcula la recompensa
            float reward = ComputeReward(newAgentPos, newOtherPos);

            if (train)
            {
                UpdateQ(stateKey, action, reward, nextStateKey);
            }

            // actualiza las posiciones
            _agentPosition = newAgentPos;
            _otherPosition = newOtherPos;

            // Actualizamos estadísticas de recompensas
            CurrentStep++;
            _return += reward;
            _returnAveraged = (_returnAveraged * (CurrentStep - 1) + reward) / CurrentStep;

            // Comprobación de si estamos en el fin de episodio
            if (IsTerminalState(_agentPosition, _otherPosition))
            {
                EndEpisode();
            }
        }

        #region Parte a implementar por el alumno

        private string BuildStateKey(CellInfo agent, CellInfo other)
        {
            var state = new QState(agent, other, _worldInfo);
            return state.ToKey();
        }


        /// <summary>
        /// Ejemplo orientativo:
        ///    - Si train == false, puedes usar la mejor acción.
        ///    - Si train == true, con probabilidad epsilon elegir acción aleatoria,
        ///      y con probabilidad 1-epsilon la mejor según _qTable.GetBestAction(stateKey).
        /// </summary>
        private QAction ChooseAction(string stateKey, bool train)
        {
            // TODO (alumno):
            // 1. Si !train -> return _qTable.GetBestAction(stateKey);

            if (!train)
            {
                // Explotar: elegir la mejor acción conocida
                return _qTable.GetBestAction(stateKey);
            }

            // 2. Si train:
            //    - double r = _random.NextDouble();
            //    - si r < _params.epsilon -> acción aleatoria
            //    - si no -> _qTable.GetBestAction(stateKey)

            // Entrenamiento: ε-greedy
            double r = _random.NextDouble();
            if (r < _params.epsilon)
            {
                // Acción aleatoria
                Array actions = Enum.GetValues(typeof(QAction));
                return (QAction)actions.GetValue(_random.Next(actions.Length));
            }
            else
            {
                // Mejor acción según QTable
                return _qTable.GetBestAction(stateKey);
            }

        }

        /// <summary>
        /// Actualización de Q-Learning:
        /// Q(s,a) = (1 - alpha) * Q(s,a) + alpha * (reward + gamma * max_a' Q(s',a')).
        /// Usa _qTable.GetQ, _qTable.SetQ y _qTable.GetMaxQ.
        /// </summary>
        private void UpdateQ(string stateKey, QAction action, float reward, string nextStateKey)
        {
            // TODO (alumno):
            // float oldQ = _qTable.GetQ(stateKey, action);
            // float maxQNext = _qTable.GetMaxQ(nextStateKey);
            // float target = reward + _params.gamma * maxQNext;
            // float newQ = (1 - _params.alpha) * oldQ + _params.alpha * target;
            // _qTable.SetQ(stateKey, action, newQ);

            // ME LO HA DADO COPIADO TAL CUAL LOL NO SÉ SI HABRÁ QUE CAMBIARLO UN POCO
            float oldQ = _qTable.GetQ(stateKey, action);
            float maxQNext = _qTable.GetMaxQ(nextStateKey);

            float target = reward + _params.gamma * maxQNext;
            float newQ = (1 - _params.alpha) * oldQ + _params.alpha * target;

            _qTable.SetQ(stateKey, action, newQ);

        }

        /// <summary>
        /// Función de recompensa.
        /// Ejemplo orientativo:
        ///   si agent == other -> recompensa positiva grande (captura)
        ///   si no -> pequeña penalización negativa por cada paso.
        /// </summary>
        private float ComputeReward(CellInfo agent, CellInfo other)
        {
            // TODO (alumno).
            // Ejemplo orientativo:
            // if (agent == other) return 10f;
            // else return -0.01f;


            // Si el player alcanza al agente -> recompensa negativa grande
            if (agent.x == other.x && agent.y == other.y)
                return -100f;

            float reward = 1f; //sobrevivir un paso
            
            // Distancia REAL después del movimiento
            int newDist = Math.Abs(agent.x - other.x)+ Math.Abs(agent.y - other.y);

            // Distancia ANTERIOR (guardada antes del step)
            int oldDist = Math.Abs(_agentPosition.x - _otherPosition.x) + Math.Abs(_agentPosition.y - _otherPosition.y);

            // Cambio de distancia
            int delta = newDist - oldDist;

            if (delta > 0)
                reward += 5f;      // se aleja
            else if (delta < 0)
                reward -= 5f;      // se acerca

            //Penalización si esta quieto
            if (agent.x == _agentPosition.x && agent.y == _agentPosition.y)
               reward -= 5.0f;

            //Incentivos a explorar
            HashSet<(int, int)> visitedPositions = new HashSet<(int, int)>();
            var pos = (agent.x, agent.y);
            if (!visitedPositions.Contains(pos))
            {
                reward += 2f;  // incentiva explorar
                visitedPositions.Add(pos);
            }


            return reward;

            // Paso normal -> pequeña penalización para motivar escapar
            //return -0.1f;

        }

        /// <summary>
        /// Condición de final de episodio.
        /// Lo más simple: cuando agente y oponente están en la misma celda.
        /// También puedes definir una probabilidad para el parámetro v visto en clase.
        /// </summary>
        private bool IsTerminalState(CellInfo agent, CellInfo other)
        {
            // TODO (alumno):
            // return agent == other;

            // Termina episodio si player alcanza al agente
            return agent.x == other.x && agent.y == other.y;

        }


        private CellInfo ApplyAction(CellInfo agentCell, QAction action)
        {
            int nx = agentCell.x;
            int ny = agentCell.y;

            switch (action)
            {
                case QAction.Up: ny += 1; break;
                case QAction.Down: ny -= 1; break;
                case QAction.Right: nx += 1; break;
                case QAction.Left: nx -= 1; break;
                case QAction.Stay: return agentCell;
            }

            //Detectar fuera del mundo
            if (nx < 0 || ny < 0 ||
                nx >= _worldInfo.WorldSize.x ||
                ny >= _worldInfo.WorldSize.y)
                return agentCell;

            var nextCell = _worldInfo[nx, ny];

            //Detectar muros
            if (!nextCell.Walkable)
                return agentCell;

            return nextCell;
        }



        private CellInfo MoveOpponent(CellInfo opponent, CellInfo target)
        {
            var path = _navigationAlgorithm.GetPath(opponent, target, 1);
            if (path.Length > 0)
                return path[0];

            return opponent;
        }
        #endregion
    }
}
