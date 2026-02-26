# Subgraph
```mermaid
---
title: Nodes with Text
---
%%{init: {'theme':'forest'}}%%
 graph LR   
    C
    --> 
    D[[D Estimate Load Distribution]]
    
    C --> SJ
    F[F Iterate Lidar Data Points] --> G{G Is in A/B Section?}
    
    subgraph SJ ["Actual Deflection
              [Calculation]" 'calc']
       direction
       G -- Yes --> H[H Calculate Load P at Corresponding Position]
             
       subgraph ad [" "]
        direction TB
       	G x-- LINK-=TEXT --> F
       end       
    end
```

```mermaid
graph LR
   subgraph Result Processing
        G -->|No| D
        G -->|Yes| H(H: Result Analysis)
        H --> I(I: Visualize Results)
    end    

    subgraph Initialization
        A[A: Initialize System] --> B(B: Set Simulation and Control Parameters)
        B --> C(C: Initialize AUV State and Environmental Parameters)
    end
    
    subgraph Main Loop
        D{D: Main Loop} --> E(E: Update State and Time)
        E --> F(F: Calculate Control Output)
        F --> G{G: End Loop?}
    end 
    
    C --> D
```

```mermaid
%%{init: {'theme':'neutral'}}%%
graph TB
    subgraph TL [Thrust Control]
        C --> D[D Calculate Speed Error]
        D --> E[E Calculate PID Control Output]
        E --> F[F Adjust Engine Speed]
        F --> G[G Calculate Thrust]
    end
    subgraph "Position Control"
        K
        C
        I -->|Reached| J[J Check if Target Position Reached]
        subgraph Inner
        	K-->C
        end
    end

    A[A Start] --> B[B Set Expected Speed and Target Position]
    B --> TL
    G --> H[H Update Equipment Speed]
    H --> I[I Check if Expected Speed Reached]
    I -->|Not Reached| C[C Read Current Speed and Position]
    J -->|Not Reached| K[K Adjust Direction and Speed]
    J -->|Reached| L[L End]
```



## Comment

`````mermaid
%%{init: {"flowchart": {"htmlLabels": false}, 'theme':'dark'} }%%
graph RL
	%% this is a comment
    %%
    A%%
    
    %%
    -- Text1 ---
    A%%
    
    D""
    `` x==x E --> A%%
    
    A%% -->|Text2|D""
`````

# Edge and Shape

````mermaid
%%{init: {'theme':'base'}}%%
graph RL
    - -.-x D
    
    A ==o xD
    A-o--xD   
    
    Ax--xD
    Ax--text0
    -->
    A --"text1"
    text2
    --> E 
    
    D --> E{"`E{Construct Ideal Deflection
	Curve **Model**}`"}
    
    D[
    D:lonely
    ]
        
    : <-- to --> E ~~~~ F
`````


# AMP

```````mermaid
graph BT
    &A & B --> C
    C-->D
```````

# Style

```mermaid
graph LR
	classDef className fill:#f9f,stroke:#333,stroke-width:4px;
	class A className;
	style A-B fill:#bbf,stroke:#f66,stroke-width:2px,color:#fff,stroke-dasharray: 5 5
	A-B-->B@{shape: rounded, label: 'A: File, Process'}
```

# HTML

```mermaid
graph LR
    A[Determine Target Distance D] --> B[Calculate Navigation Distance D<sub>nav</sub>]
    B --> C{D ≤ D<sub>nav</sub> ?}
    C -->|Yes| D[Calculate Self-Destruct Damage H]
    D --> E[Execute Impact Action]
    C -->|No| F[Continue Searching for Target or Take Other Actions]
```

# Flowchart Shape Samples

```mermaid
graph LR
    R@{ shape: rect } --> T@{ shape: text }
    O@{ shape: rounded } --> Tri@{ shape: tri }
    Di@{ shape: diam } --> Hex@{ shape: hex }
    Cy@{ shape: cyl } --> HC@{ shape: h-cyl }
    L1@{ shape: lean-l } --> L2@{ shape: lean-r }
    Tb@{ shape: trap-b } --> Tt@{ shape: trap-t }
    Card@{ shape: card } --> R

    D -- aS --> n1>"asd"]
    D -- aS --> n2(["asd"])
```
