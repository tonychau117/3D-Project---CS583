using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Tools
{
    /// <summary> Class used for implementing A* Search, which is used for Enemy Pathfinding </summary>
    public static class AStarSearch
    {
        /* * * * * * * * * *
         * A* Main Methods *
         * * * * * * * * * */
        
        /// <summary> Node that represents a position in A* Search </summary>
        public class AStarNode
        {
            public Vector3 position { get; }
            public AStarNode parent { get; } // The node that the current node was reached from
            public float fScore { get; } // Score representing how close the position is to the goal
            public float gScore { get; } // Score representing how much it took to get to this point from the start position

            // Constructor
            public AStarNode(Vector3 p_position, AStarNode p_parent, float p_fScore, float p_gScore)
            {
                position = p_position;
                parent = p_parent;
                fScore = p_fScore;
                gScore = p_gScore;
            }
        }

        /// <summary> Use A* Search to find a path between a Starting and Goal position </summary>
        public static Stack<Vector3> Search(Transform entity, Vector3 goal, float threshold = 1, 
            int epochs = 500, bool useFloorPos = false)
        {
            //HEADER: __Initializations__ 
            
            var start = useFloorPos ? entity.Find("FloorLevel").position : entity.position; // The Entity's Starting Position
            var size = entity.localScale; // Size of the entity
            
            var openList = new List<AStarNode>(); // Queue for storing unchecked positions
            var closedList = new List<AStarNode>(); // Queue for storing already checked positions
            
            var roundedStart = RoundPosition(start); // Round the starting position to the nearest integers
            var startingNode = new AStarNode(roundedStart, null, 0, 0); // The node at the starting position

            //HEADER ___Main Loop___

            openList.Add(startingNode); // Add the starting node to the open list

            AStarNode goalNode = null; // The found goal node

            var timeout = false;
            
            while (openList.Count > 0 && goalNode.IsUnityNull() && !timeout) // While the list is not empty
            {

                if (closedList.Count > epochs) { timeout = true; continue;}
                
                //SUBHEADER ___Obtain Successors___
                var q = Pop(openList); // Pop from Open List
                var successors = GetSuccessors(q.position);
                
                // For each successor position that is valid for the enemy to move into
                foreach (var successor in successors.Where(successor => IsValidPosition(successor, size)))
                {
                    //SUBHEADER ___Goal is Found___
                    
                    // Check if the distance from the goal position is below a threshold. If so, set this successor as the goal node 
                    if (Vector3.Distance(successor, goal) < threshold)
                    {
                        goalNode = new AStarNode(successor, q, 0, 999); break;
                    } 
                    
                    //SUBHEADER ___Compute G, H, & F___

                    var g = q.gScore + Vector3.Distance(q.position, successor); // How far from the start node is this node?
                    var h = Vector3.Distance(successor, goal); // How close to the goal is this node?
                    var f = g + h; // Combine them both to get the F Score (the lower the score, the better the path)
                    
                    // SUBHEADER ___Check Open and Closed List___

                    var combinedList = openList.Concat(closedList).ToList(); // Combine both lists, so we can check both at the same time
                    // Skip this node if another with the same position already exists and has a lower f score
                    if (combinedList.Any(node => CheckForNode(node, successor, f))) continue;
                    
                    // SUBHEADER ___Add New Node___
                    
                    var newNode = new AStarNode(successor, q, f, g);
                    AddNode(newNode, openList);
                }
                closedList.Add(q); // Add the checked node to the end of the closedLIst
            }

            // Using the created "Parent-Trail", make a path between the start and goal
            return !goalNode.IsUnityNull() ? CreatePath(goalNode, startingNode) : null;
        }

        
        /* * * * * * * * * * *
         * A* Helper Methods *
         * * * * * * * * * * */
        
        /// <summary> Return the 8 surrounding successors of a given position </summary>
        private static List<Vector3> GetSuccessors(Vector3 pos)
        {
            var successors = new List<Vector3>(); // List to fill and return
            
            // Loop through the 3x3 offset square around the pos. Get every position except pos itself
            for (var i = -1; i < 2; i++) {
                for (var j = -1; j < 2; j++)
                {
                    if (i == 0 && j == 0) continue; // Skip the (0,0) offset (which is just Pos)
                    var successor = pos + new Vector3(i, 0, j);  
                    successors.Add(successor); 
                }
            }

            return successors;
        }
        
        /// <summary> Determine if a position is valid to move to by using a OverlapBox to determine if any obstacles are at that position.</summary>
        /// <param name="pos">The position of the BoxCast</param>
        /// <param name="size">The size of the BoxCast</param>
        private static bool IsValidPosition(Vector3 pos, Vector3 size)
        {
            var halfExtents = new Vector3(size.x / 2, size.y / 2, size.z / 2); // Determine size of BoxCast
            
            // Get all the colliders at this position
            // ReSharper disable once Unity.PreferNonAllocApi
            var hits = PhyTools.OverlapBox(pos, halfExtents, Color.yellow).NotNull().ToList();
            return hits.Count <= 0 || // Position is valid to move to if no colliders are found, or
                   // If none of the colliders are an obstacle
                   hits.All(hit => hit.transform.CompareTag("Player") || hit.transform.CompareTag("Floor"));
        }

        /// <summary> Add a node to the open list, whilst maintaining an ascending order of f </summary>
        private static void AddNode(AStarNode node, List<AStarNode> list)
        {
            // Search the entire list
            for (var index = 0; index < list.Count; index++)
            {
                var compNode = list[index]; 
                // Insert once a comparison node with a higher f score than the inserted node is found
                if (!(node.fScore < compNode.fScore)) continue;
                list.Insert(index, node);
                return;
            }
            list.Add(node); // If every element in the list has a smaller f score, add the node to the end
        }

        /// <summary> Create a path from the start node to the goal node using the "parent-trail" developed by the Search algorithm </summary>
        private static Stack<Vector3> CreatePath(AStarNode goalNode, AStarNode startNode)
        {
            var finalPath = new Stack<Vector3>(); // Create the stack (LIFO)
            var curNode = goalNode; // Start with the Goal Node, and work backwards
            while (curNode != startNode) // While the start node has not been reached (No need to include the start node)
            {
                finalPath.Push(curNode.position); // Push each position to the stack
                curNode = curNode.parent; // The parent of the current node contains the next position to push to the stack
            }
            return finalPath;
        }

        /// <summary> If a node with the same position has been checked, and has a lower fScore, disregard the new node</summary>
        private static bool CheckForNode(AStarNode node, Vector3 successor, float f)
        {
            return node.position == successor // Both nodes have the same position
                   && node.fScore <= f; // The previous node's F Score is lower than the new one
        }
        
        
        /* * * * * * * * * *
         * Vector3 Methods *
         * * * * * * * * * */
        
        /// <summary> Return the given position rounded to the nearest integers </summary>
        private static Vector3 RoundPosition(Vector3 pos) { return new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z)); }
        
        
        /* * * * * * * * * 
         * Queue Methods *
         * * * * * * * * */
        
        /// <summary> Remove a value from the front of a list, and return it </summary>
        /// <typeparam name="T"> The data type of the List </typeparam>
        private static T Pop<T>(List<T> list)
        {
            var node = list[0]; //  Grab the first element in the list
            list.RemoveAt(0); // Remove the first element in the list
            return node;
        }
    }
}
