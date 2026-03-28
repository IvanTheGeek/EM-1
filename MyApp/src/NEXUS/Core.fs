module NEXUS.Core

open System

// ======================================================
// NEXUS CORE
// ======================================================
//
// NEXUS.Core is the universal substrate.
//
// It does NOT model one specific lens such as:
// - Event Modeling
// - UI
// - accounting
// - knowledge
// - archive (TOML files) 
//
// Instead, it models the smallest useful truth layer:
// - identities
// - nodes
// - relations
// - graph
// - core invariants
//
// Everything more specific should be built as a lens over this.
//
// Design intent:
// - immutable
// - pure
// - universal
// - relation-aware
// - safe by default
//
// ======================================================



// ======================================================
// IDENTITY
// ======================================================
//
// Identity is foundational.
// Every node, relation, and graph must have a stable identity.
//
// UUIDv7 is the default minting strategy.
// Why UUIDv7?
// - globally unique
// - naturally sortable by minting order
// - useful for timeline-aware and append-oriented systems
//
// Important:
// UUIDv7 ordering reflects ID creation order,
// NOT necessarily domain/business time.
// If domain time matters, model it explicitly elsewhere.
//
// These wrappers prevent accidental misuse, such as:
// - passing a NodeId where a RelationId is expected
// - treating raw Guid as interchangeable everywhere
//

type NodeId = private NodeId of Guid
type RelationId = private RelationId of Guid
type GraphId = private GraphId of Guid

[<RequireQualifiedAccess>]
module Id =

    let private newGuidV7 () =
        Guid.CreateVersion7()

    let newNodeId () =
        NodeId (newGuidV7())

    let newRelationId () =
        RelationId (newGuidV7())

    let newGraphId () =
        GraphId (newGuidV7())

    let valueOfNodeId (NodeId value) = value
    let valueOfRelationId (RelationId value) = value
    let valueOfGraphId (GraphId value) = value



// ======================================================
// RELATION SEMANTICS
// ======================================================
//
// NEXUS is relation-aware.
//
// A node in isolation may exist,
// but structure, flow, and meaning emerge through relations.
//
// This is why relations are explained before nodes.
// The graph is not just "a bag of things".
// It is a network of connected meaning.
//
// RelationKind answers:
// "What kind of connection exists between these two nodes?"
//

type RelationKind =
    | Relates      // General connection
    | Contains     // Whole/part or parent/child
    | References   // Points to, mentions, depends on identity of
    | Transforms   // One thing is transformed into another shape/state
    | Projects     // A shaped representation derived from another thing
    | Influences   // Affects, informs, or shapes another thing
    | Precedes     // Ordered before; sequence or timeline relation
    | Refines      // More specific, evolved, or narrowed form



// ======================================================
// NODE SEMANTICS
// ======================================================
//
// Nodes represent "things we want to identify in the graph".
//
// NodeKind is intentionally generic.
// It should remain useful across many domains.
//
// These are NOT intended to capture every possible domain concept.
// They are core categories that later lenses can specialize.
//

type NodeKind =
    | Thing        // A concrete or identifiable thing
    | Concept      // An idea, abstraction, classification, or meaning
    | Role         // A participant perspective or responsibility
    | State        // A condition, status, or situation
    | Record       // A captured fact or documented item
    | Artifact     // A concrete output, file, screen, report, etc.
    | Lens         // A shaping interpretation over the graph



// ======================================================
// CORE RELATION STRUCTURE
// ======================================================
//
// A relation connects two nodes.
//
// Source and Target are directional.
// Direction matters because many meanings depend on orientation.
//
// Example:
// A ──Projects──► B
//
// is not the same as
//
// B ──Projects──► A
//

type Relation =
    { Id: RelationId
      Kind: RelationKind
      Source: NodeId
      Target: NodeId }



// ======================================================
// CORE NODE STRUCTURE
// ======================================================
//
// A node has:
// - stable identity
// - a generic kind
// - a human-readable name
//
// Name is NOT identity.
// Two nodes may have similar names but different identities.
//

type Node =
    { Id: NodeId
      Kind: NodeKind
      Name: string }



// ======================================================
// GRAPH
// ======================================================
//
// The graph is the truth container.
//
// It stores:
// - nodes
// - relations
//
// It contains no UI layout,
// no rendering data,
// no framework-specific concerns.
//
// Those belong in outer layers.

type Graph =
    { Id: GraphId
      Nodes: Map<NodeId, Node>
      Relations: Map<RelationId, Relation> }



// ======================================================
// ERRORS / INVALID STATES
// ======================================================
//
// Invalid states should be explicit.
//
// Instead of letting bad states silently enter the graph,
// we model failures as data.
//
// This keeps the core honest and testable.

type GraphError =
    | EmptyName
    | DuplicateNodeName of string
    | MissingSourceNode of NodeId
    | MissingTargetNode of NodeId
    | InvalidSelfRelation of RelationKind * NodeId
    | DuplicateRelation of RelationKind * NodeId * NodeId
    | InvalidRelation of RelationKind * NodeKind * NodeKind


[<AutoOpen>]
module private InternalBuilders =
    type ResultBuilder() =
        member _.Return(x) = Ok x
        member _.ReturnFrom(m: Result<'T, 'E>) = m
        member _.Bind(m, f) = Result.bind f m
        member _.Zero() = Ok ()
        member _.Combine(m, f) = Result.bind f m
        member _.Delay(f) = f
        member _.Run(f) = f()

    let result = ResultBuilder()



// ======================================================
// PRIVATE HELPERS
// ======================================================

[<RequireQualifiedAccess>]
module private Validation =

    let normalizeName (name: string) =
        name.Trim()

    let requireNonEmptyName name =
        let normalized = normalizeName name
        if String.IsNullOrWhiteSpace normalized then
            Error EmptyName
        else
            Ok normalized

    let nodeNameExists name (graph: Graph) =
        graph.Nodes
        |> Map.exists (fun _ node -> String.Equals(node.Name, name, StringComparison.OrdinalIgnoreCase))

    let relationExists kind sourceId targetId (graph: Graph) =
        graph.Relations
        |> Map.exists (fun _ relation ->
            relation.Kind = kind
            && relation.Source = sourceId
            && relation.Target = targetId)

    let tryFindNode nodeId (graph: Graph) =
        Map.tryFind nodeId graph.Nodes

    let requireNodeExists missingError nodeId (graph: Graph) =
        match tryFindNode nodeId graph with
        | Some node -> Ok node
        | None -> Error (missingError nodeId)

    let allowSelfRelation kind =
        match kind with
        | Relates
        | References -> true
        | Contains
        | Transforms
        | Projects
        | Influences
        | Precedes
        | Refines -> false

    let isRelationAllowed sourceKind targetKind relationKind =
        match relationKind, sourceKind, targetKind with
        | Relates, _, _ -> true
        | References, _, _ -> true

        | Contains, Thing, Thing -> true
        | Contains, Concept, Concept -> true
        | Contains, Artifact, Artifact -> true

        | Transforms, Thing, Thing -> true
        | Transforms, State, State -> true
        | Transforms, Record, Record -> true

        | Projects, Record, Artifact -> true
        | Projects, Concept, Artifact -> true
        | Projects, Lens, Artifact -> true

        | Influences, Artifact, Role -> true
        | Influences, Record, Role -> true
        | Influences, Concept, Role -> true

        | Precedes, State, State -> true
        | Precedes, Record, Record -> true
        | Precedes, Artifact, Artifact -> true

        | Refines, Concept, Concept -> true
        | Refines, Lens, Lens -> true
        | Refines, Artifact, Artifact -> true

        | _ -> false



// ======================================================
// CONSTRUCTORS
// ======================================================
//
// Smart constructors centralize creation rules.
// This is where "invalid states harder to reach" begins.
//

[<RequireQualifiedAccess>]
module Node =

    let create kind name : Result<Node, GraphError> =
        result {
            let! validName = Validation.requireNonEmptyName name
            return
                { Id = Id.newNodeId()
                  Kind = kind
                  Name = validName }
        }

[<RequireQualifiedAccess>]
module Relation =

    let create kind sourceId targetId : Result<Relation, GraphError> =
        if sourceId = targetId && not (Validation.allowSelfRelation kind) then
            Error (InvalidSelfRelation (kind, sourceId))
        else
            Ok
                { Id = Id.newRelationId()
                  Kind = kind
                  Source = sourceId
                  Target = targetId }

[<RequireQualifiedAccess>]
module Graph =

    let empty () : Graph =
        { Id = Id.newGraphId()
          Nodes = Map.empty
          Relations = Map.empty }



// ======================================================
// GRAPH OPERATIONS
// ======================================================
//
// These functions operate on the truth graph.
// They remain pure and return either:
// - a new valid graph
// - an explicit error
//
// This separation is intentional.
// It protects the core from becoming polluted
// by one methodology, tool, or presentation format.

[<RequireQualifiedAccess>]
module GraphOps =

    let addNode (node: Node) (graph: Graph) : Result<Graph, GraphError> =
        if Validation.nodeNameExists node.Name graph then
            Error (DuplicateNodeName node.Name)
        else
            Ok
                { graph with
                    Nodes = graph.Nodes |> Map.add node.Id node }

    let addRelation (relation: Relation) (graph: Graph) : Result<Graph, GraphError> =
        result {
            let! sourceNode =
                Validation.requireNodeExists MissingSourceNode relation.Source graph

            let! targetNode =
                Validation.requireNodeExists MissingTargetNode relation.Target graph

            if Validation.relationExists relation.Kind relation.Source relation.Target graph then
                return! Error (DuplicateRelation (relation.Kind, relation.Source, relation.Target))

            if not (Validation.isRelationAllowed sourceNode.Kind targetNode.Kind relation.Kind) then
                return! Error (InvalidRelation (relation.Kind, sourceNode.Kind, targetNode.Kind))

            return
                { graph with
                    Relations = graph.Relations |> Map.add relation.Id relation }
        }

    let tryFindNode (nodeId: NodeId) (graph: Graph) : Node option =
        graph.Nodes |> Map.tryFind nodeId

    let tryFindRelation (relationId: RelationId) (graph: Graph) : Relation option =
        graph.Relations |> Map.tryFind relationId

    let outgoingRelations (nodeId: NodeId) (graph: Graph) : Relation list =
        graph.Relations
        |> Map.values
        |> Seq.filter (fun relation -> relation.Source = nodeId)
        |> Seq.toList

    let incomingRelations (nodeId: NodeId) (graph: Graph) : Relation list =
        graph.Relations
        |> Map.values
        |> Seq.filter (fun relation -> relation.Target = nodeId)
        |> Seq.toList

    let connectedNodes (nodeId: NodeId) (graph: Graph) : Node list =
        let relatedIds =
            seq {
                for relation in graph.Relations.Values do
                    if relation.Source = nodeId then yield relation.Target
                    if relation.Target = nodeId then yield relation.Source
            }
            |> Seq.distinct

        relatedIds
        |> Seq.choose (fun id -> tryFindNode id graph)
        |> Seq.toList

    let nodeCount (graph: Graph) = graph.Nodes.Count
    let relationCount (graph: Graph) = graph.Relations.Count