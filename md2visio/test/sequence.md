# Sequence Diagram Test

## Basic Sequence Diagram - User Login Flow

```mermaid
sequenceDiagram
    participant a as User
    participant b as Browser
    participant c as Server
    participant d as Database

    a->>b: Enter username and password
    b->>c: Send login request
    activate c
    c->>c: Preprocess and validate received data
    c->>d: Verify user information
    activate d
    d-->>c: User information valid
    deactivate d
    c-->>b: Login successful, return token
    deactivate c
    b-->>a: Show login success page
```

## Complex Sequence Diagram - Order Processing Flow

```mermaid
sequenceDiagram
    participant user as User
    participant web as Web Frontend
    participant api as API Gateway
    participant order as Order Service
    participant payment as Payment Service
    participant inventory as Inventory Service

    user->>web: Create Order
    web->>api: POST /orders
    api->>order: Create order request
    
    activate order
    order->>inventory: Check inventory
    activate inventory
    inventory-->>order: Inventory sufficient
    deactivate inventory
    
    order->>order: Generate order number
    order->>payment: Initiate payment
    
    activate payment
    payment->>payment: Process payment logic
    payment-->>order: Payment successful
    deactivate payment
    
    order-->>api: Order created successfully
    deactivate order
    api-->>web: Return order info
    web-->>user: Show order confirmation page
```

## Self-call Test

```mermaid
sequenceDiagram
    participant sys as System
    participant cache as Cache Service

    sys->>cache: Query data
    cache->>cache: Check if cache is valid
    cache->>cache: Clear expired cache
    cache-->>sys: Return cached data
```

## Simple Message Type Test

```mermaid
sequenceDiagram
    participant A
    participant B
    participant C

    A->B: Simple message
    B->>C: Synchronous message
    C-->A: Asynchronous return
    A-->>B: Dashed synchronous message
```

## Fragment and Note Test - Receive Timeout Handling

```mermaid
sequenceDiagram
    participant Device as External Device
    participant USART as USART1_RDR
    participant DMA as DMA1_CH0
    participant RX_Buf as data_buffer_USART1_RX
    participant TMR as TMR0_Ch_A
    participant ISR as Interrupt Handler
    participant Parse as usart1_frame_parse_optimized
    participant Event as Async Event System

    Device->>USART: Serial receive data
    USART->>DMA: Trigger DMA transfer (EVT_SRC_USART1_RI)
    DMA->>RX_Buf: Automatically store data

    alt Receive Timeout
        TMR-->>ISR: TMR0 timeout interrupt
        note over ISR: USART1_RxTimeout_IrqCallback
        ISR->>DMA: Stop DMA
        ISR->>ISR: Calculate receive length
        ISR->>Parse: Call frame parsing
        Parse->>Parse: Validate frame header (0xAA 0x55)
        Parse->>Parse: Validate frame tail (0xED)
        Parse->>Parse: XOR checksum
        Parse->>Event: SAFE_EVENT_EMIT(UART1_RECEIVE)
    else DMA Transfer Complete
        DMA-->>ISR: DMA_TC interrupt
        note over ISR: USART1_RX_DMA_TC_IrqCallback
        ISR->>DMA: Reset DMA
    end

    ISR->>DMA: Re-enable DMA receive
```
