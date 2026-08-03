using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jeomseon.GameObjectPooling.Definitions
{
    /// <summary>
    /// Groups pool definitions that a GameObjectPoolScope registers and prewarms together.
    /// GameObjectPoolScope가 함께 등록하고 프리웜할 풀 Definition을 묶습니다.
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(GameObjectPoolCatalog),
        menuName = "Tool/GameObject Pooling/GameObject Pool Catalog")]
    public sealed class GameObjectPoolCatalog : ScriptableObject
    {
        [SerializeField] private GameObjectPoolDefinition[] _definitions =
            Array.Empty<GameObjectPoolDefinition>();

        /// <summary>
        /// Gets the definitions registered in this catalog.
        /// 이 Catalog에 등록된 Definition을 가져옵니다.
        /// </summary>
        public IReadOnlyList<GameObjectPoolDefinition> Definitions => _definitions;
    }
}
