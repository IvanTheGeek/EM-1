module NEXUS.Core

open System

// ======================================================
// NEXUS CORE
// ======================================================
//
// NEXUS.Core is the universal substrate.
//
// It does NOT model one specific domain such as:
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
// UUIDv7 is the intended default strategy for minting IDs.
// Why UUIDv7?
// - globally unique
// - sortable by creation order
// - friendly to append-oriented and timeline-aware systems
//
// Important:
// UUIDv7 ordering reflects ID minting order,
// NOT necessarily domain/business time.
// If domain time matters, model it explicitly elsewhere.
//
// These wrappers prevent accidental misuse, such as:
// - passing a NodeId where a RelationId is expected
// - treating raw Guid as interchangeable everywhere
//

type RelationId = private RelationId of Guid
type NodeId = private NodeId of Guid
type GraphId = private GraphId of Guid


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
//

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
//

type GraphError =
    | EmptyName
    | DuplicateNodeName of string
    | MissingSourceNode of NodeId
    | MissingTargetNode of NodeId
    | InvalidSelfRelation of RelationKind * NodeId


// ======================================================
// HUMAN CONTEXT
// ======================================================
//
// How this maps to the larger NEXUS idea:
//
// - The graph is truth
// - Lenses are interpretations over the graph
// - Artifacts are outputs shaped from the graph
//
// So:
//
// - Event Modeling is not the core graph
//   It is a lens over the core graph
//
// - A UI is not the core graph
//   It is a lens over the core graph
//
// - A TOML archive is not the core graph
//   It is a lens over the core graph
//
// This separation is intentional.
// It protects the core from becoming polluted
// by one methodology, tool, or presentation format.
//
// ======================================================