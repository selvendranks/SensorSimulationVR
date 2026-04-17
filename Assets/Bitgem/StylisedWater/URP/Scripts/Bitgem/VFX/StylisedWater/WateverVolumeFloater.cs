#region Using statements

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Bitgem.VFX.StylisedWater
{
    public class WateverVolumeFloater : MonoBehaviour
    {
        #region Public fields

        public WaterVolumeHelper WaterVolumeHelper = null;

        #endregion

        #region Serialized fields

        // Visible in Inspector, but private in code
        [SerializeField] private float heightOffset = 0.5f;

        #endregion

        #region MonoBehaviour events

        void Update()
        {
            var instance = WaterVolumeHelper ? WaterVolumeHelper : WaterVolumeHelper.Instance;
            if (!instance)
            {
                return;
            }

            // Get water height at this position
            float waterHeight = instance.GetHeight(transform.position) ?? transform.position.y;

            // Apply offset so object floats higher
            transform.position = new Vector3(
                transform.position.x,
                waterHeight + heightOffset,
                transform.position.z
            );
        }

        #endregion
    }
}
