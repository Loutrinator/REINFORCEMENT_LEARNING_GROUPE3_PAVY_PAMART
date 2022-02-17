﻿using System;
using UnityEngine;

namespace ReinforcementLearning {
    [Serializable]
    public class GameGrid {
        public GridState gridState;

        public void Init(Transform groundPlane) {
            var groundScale = groundPlane.lossyScale;
            var groundSize = new Vector2(groundScale.x * 10, groundScale.y * 10);
            var groundPos = groundPlane.position;
            var firstPos = new Vector3(groundPos.x - groundSize.x / 2, groundPos.y - groundSize.y / 2, 0);

            string log = "";
            var grid = new int[(int)groundSize.y][];
            for (int i = grid.Length - 1; i >= 0; --i) {
                grid[i] = new int[(int)groundSize.x];

                for (int j = 0; j < grid[i].Length; ++j) {
                    Vector3 castPos = firstPos + j * Vector3.right + i * Vector3.up + new Vector3(0.5f, 0.5f, 0);
                    if (Physics.Raycast(castPos + Vector3.back * 5, Vector3.forward, out var hit, 10)) {
                        var objLayer = hit.collider.gameObject.layer;
                        if (objLayer == LayerMask.NameToLayer("Ground")) {
                            grid[i][j] = 0;
                        }
                        if (objLayer == LayerMask.NameToLayer("Wall")) {
                            grid[i][j] = 1;
                        }
                        if (objLayer == LayerMask.NameToLayer("Player")) {
                            grid[i][j] = 2;
                        }
                        if (objLayer == LayerMask.NameToLayer("Arrival")) {
                            grid[i][j] = 3;
                        }
                    }
                    else {
                        grid[i][j] = -1;
                    }
                
                    log = log + grid[i][j] + " ";
                }

                log += "\n";
            }

            gridState = new GridState(grid);
            Debug.Log(log);
        }
    }

    public static class GridUtils {
        public static int[][] CloneGrid(this int[][] source) {
            var result = new int[source.Length][];

            for (int i = 0; i < source.Length; ++i) {
                result[i] = new int[source[i].Length];
                for (int j = 0; j < source[i].Length; ++j) {
                    result[i][j] = source[i][j];
                }
            }
        
            return result;
        }
    }
}