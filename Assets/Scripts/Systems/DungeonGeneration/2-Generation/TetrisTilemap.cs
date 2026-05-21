using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TetrisTile
{
    public int Id;

    public HashSet<Vector2Int> Cells = new();

    public HashSet<int> Neighbors = new();

    public TetrisTile(int id)
    {
        Id = id;
    }

    public void AddCell(Vector2Int cell)
    {
        Cells.Add(cell);
    }
}

public class TetrisTilemap
{
    /*
     * Grid accessed as:
     * grid[x, y]
     */

    public MapConfig Config;

    public int?[,] Grid;

    public Dictionary<int, TetrisTile> Tiles = new();

    private System.Random rng = new();

    public TetrisTilemap(MapConfig config)
    {
        Config = config;

        Reset();
    }

    public TetrisTilemap Generate()
    {
        Reset();

        int tileId = 0;

        while (true)
        {
            List<Vector2Int> unassigned =
                GetUnassigned();

            if (unassigned.Count == 0)
                break;

            Vector2Int seed =
                PickLowestUnassigned(unassigned);

            HashSet<Vector2Int> region =
                GetTilesCells(seed);

            // Small leftover region
            if (region.Count <= Config.minShapeSize)
            {
                TetrisTile tile =
                    new TetrisTile(tileId);

                foreach (Vector2Int cell in region)
                {
                    Grid[(int)cell.x, (int)cell.y] =
                        tileId;

                    tile.AddCell(cell);
                }

                Tiles[tileId] = tile;

                tileId++;

                continue;
            }

            // Grow shape
            HashSet<Vector2Int> tileCells = null;

            for (
                int i = 0;
                i < Config.maxGrowthAttempts;
                i++
            )
            {
                tileCells = GrowShape(seed);

                if (tileCells != null)
                    break;
            }

            if (tileCells == null)
            {
                tileCells = new HashSet<Vector2Int>()
                {
                    seed
                };
            }

            TetrisTile newTile =
                new TetrisTile(tileId);

            foreach (Vector2Int cell in tileCells)
            {
                Grid[(int)cell.x, (int)cell.y] =
                    tileId;

                newTile.AddCell(cell);
            }

            Tiles[tileId] = newTile;

            tileId++;
        }

        BuildAdjacency();

        return this;
    }

    private void Reset()
    {
        Grid = new int?[Config.width, Config.height];

        Tiles.Clear();
    }

    private bool InBounds(Vector2Int v)
    {
        return
            v.x >= 0 &&
            v.x < Config.width &&
            v.y >= 0 &&
            v.y < Config.height;
    }

    private List<Vector2Int> GetUnassigned()
    {
        List<Vector2Int> result = new();

        for (int x = 0; x < Config.width; x++)
        {
            for (int y = 0; y < Config.height; y++)
            {
                if (Grid[x, y] == null)
                {
                    result.Add(new Vector2Int(x, y));
                }
            }
        }

        return result;
    }

    private Vector2Int PickLowestUnassigned(
        List<Vector2Int> unassigned
    )
    {
        float minY =
            unassigned.Min(v => v.y);

        List<Vector2Int> candidates =
            unassigned
                .Where(v => v.y == minY)
                .ToList();

        return candidates[
            rng.Next(candidates.Count)
        ];
    }

    private HashSet<Vector2Int> GrowShape(Vector2Int seed)
    {
        HashSet<Vector2Int> shape =
            new() { seed };

        for (
            int i = 0;
            i < Config.maxShapeSize * 4;
            i++
        )
        {
            if (shape.Count >= Config.maxShapeSize)
                break;

            HashSet<Vector2Int> candidates =
                new();

            foreach (Vector2Int cell in shape)
            {
                foreach (
                    Vector2Int n
                    in cell.Neighbors()
                )
                {
                    if (
                        InBounds(n) &&
                        Grid[(int)n.x, (int)n.y] == null
                    )
                    {
                        candidates.Add(n);
                    }
                }
            }

            if (candidates.Count == 0)
            {
                return
                    shape.Count >= Config.minShapeSize
                        ? shape
                        : null;
            }

            List<Vector2Int> candidateList =
                candidates.ToList();

            Vector2Int selected =
                candidateList[
                    rng.Next(candidateList.Count)
                ];

            shape.Add(selected);
        }

        return
            shape.Count >= Config.minShapeSize
                ? shape
                : null;
    }

    private HashSet<Vector2Int> GetTilesCells(
        Vector2Int seed
    )
    {
        Stack<Vector2Int> stack = new();

        HashSet<Vector2Int> visited = new();

        stack.Push(seed);

        while (stack.Count > 0)
        {
            Vector2Int current =
                stack.Pop();

            if (visited.Contains(current))
                continue;

            if (!InBounds(current))
                continue;

            if (
                Grid[
                    (int)current.x,
                    (int)current.y
                ] != null
            )
            {
                continue;
            }

            visited.Add(current);

            foreach (
                Vector2Int n
                in current.Neighbors()
            )
            {
                stack.Push(n);
            }
        }

        return visited;
    }

    private void AddEdge(int a, int b)
    {
        if (
            !Tiles.ContainsKey(a) ||
            !Tiles.ContainsKey(b)
        )
        {
            return;
        }

        Tiles[a].Neighbors.Add(b);

        Tiles[b].Neighbors.Add(a);
    }

    private void BuildAdjacency()
    {
        for (int x = 0; x < Config.width; x++)
        {
            for (int y = 0; y < Config.height; y++)
            {
                int? currentId =
                    Grid[x, y];

                if (currentId == null)
                    continue;

                Vector2Int current =
                    new Vector2Int(x, y);

                foreach (
                    Vector2Int n
                    in current.Neighbors()
                )
                {
                    if (!InBounds(n))
                        continue;

                    int? neighborId =
                        Grid[
                            (int)n.x,
                            (int)n.y
                        ];

                    if (
                        neighborId != null &&
                        neighborId != currentId
                    )
                    {
                        AddEdge(
                            currentId.Value,
                            neighborId.Value
                        );
                    }
                }
            }
        }
    }

    public List<int> BuildChain()
    {
        List<int> ids =
            Tiles.Keys.ToList();

        while (true)
        {
            int start =
                ids[rng.Next(ids.Count)];

            if (
                Tiles[start].Cells.Count <= 1
            )
            {
                continue;
            }

            List<int> chain =
                new() { start };

            while (
                chain.Count < Config.numCycles
            )
            {
                int current =
                    chain[^1];

                HashSet<int> neighbors =
                    Tiles[current].Neighbors;

                List<int> candidates =
                    neighbors
                        .Where(n => !chain.Contains(n)
                        )
                        .ToList();

                if (candidates.Count == 0)
                    break;

                int candidate =
                    candidates[
                        rng.Next(candidates.Count)
                    ];

                if (
                    Tiles[candidate]
                        .Cells
                        .Count <= 1
                )
                {
                    break;
                }

                chain.Add(candidate);
            }

            if (
                chain.Count ==
                Config.numCycles
            )
            {
                return chain;
            }
        }
    }

    public TetrisTilemap FilterToChain(
        List<int> chain
    )
    {
        TetrisTilemap newTilemap =
            new TetrisTilemap(Config);

        newTilemap.Grid =
            new int?[
                Config.width,
                Config.height
            ];

        // Copy tiles
        foreach (int cid in chain)
        {
            TetrisTile oldTile =
                Tiles[cid];

            TetrisTile newTile =
                new TetrisTile(cid);

            newTile.Cells =
                new HashSet<Vector2Int>(
                    oldTile.Cells
                );

            newTilemap.Tiles[cid] =
                newTile;

            foreach (
                Vector2Int cell
                in newTile.Cells
            )
            {
                newTilemap.Grid[
                    (int)cell.x,
                    (int)cell.y
                ] = cid;
            }
        }

        // Rebuild adjacency
        foreach (int cid in chain)
        {
            foreach (
                int neighbor
                in Tiles[cid].Neighbors
            )
            {
                if (chain.Contains(neighbor))
                {
                    newTilemap
                        .Tiles[cid]
                        .Neighbors
                        .Add(neighbor);
                }
            }
        }

        return newTilemap;
    }
}