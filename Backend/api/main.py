"""
OmniWorld Backend API
FastAPI server for game backend services
"""

from fastapi import FastAPI, HTTPException, WebSocket, WebSocketDisconnect
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional, Dict
import uvicorn
from datetime import datetime
import asyncio

app = FastAPI(
    title="OmniWorld API",
    description="Backend API for OmniWorld Metaverse",
    version="1.0.0"
)

# CORS middleware for Unity client
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify Unity client origin
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# In-memory storage (replace with PostgreSQL in production)
players_db = {}
transactions_db = []
properties_db = []
active_connections: List[WebSocket] = []

# Pydantic models
class Player(BaseModel):
    wallet_address: str
    username: Optional[str] = None
    balance: float = 0.0
    reputation: float = 0.5
    current_city: str = "OmniLanta"
    created_at: str = datetime.now().isoformat()

class Transaction(BaseModel):
    tx_hash: str
    from_address: str
    to_address: str
    amount: float
    transaction_type: str
    timestamp: str = datetime.now().isoformat()

class Property(BaseModel):
    token_id: int
    owner_address: str
    property_type: str
    zone_type: str
    city: str
    value: float
    metadata: Dict

class Quest(BaseModel):
    quest_id: int
    title: str
    description: str
    reward: float
    experience_reward: int
    quest_type: str
    is_active: bool = True

# Root endpoint
@app.get("/")
async def root():
    return {
        "message": "Welcome to OmniWorld API",
        "version": "1.0.0",
        "status": "operational"
    }

# Health check
@app.get("/health")
async def health_check():
    return {
        "status": "healthy",
        "timestamp": datetime.now().isoformat()
    }

# Player endpoints
@app.post("/api/players/register")
async def register_player(player: Player):
    """Register a new player"""
    if player.wallet_address in players_db:
        raise HTTPException(status_code=400, detail="Player already registered")
    
    players_db[player.wallet_address] = player.dict()
    
    return {
        "success": True,
        "message": "Player registered successfully",
        "player": player
    }

@app.get("/api/players/{wallet_address}")
async def get_player(wallet_address: str):
    """Get player information"""
    if wallet_address not in players_db:
        raise HTTPException(status_code=404, detail="Player not found")
    
    return players_db[wallet_address]

@app.put("/api/players/{wallet_address}/balance")
async def update_balance(wallet_address: str, amount: float):
    """Update player balance"""
    if wallet_address not in players_db:
        raise HTTPException(status_code=404, detail="Player not found")
    
    players_db[wallet_address]["balance"] = amount
    
    return {
        "success": True,
        "new_balance": amount
    }

@app.put("/api/players/{wallet_address}/city")
async def update_city(wallet_address: str, city: str):
    """Update player's current city"""
    if wallet_address not in players_db:
        raise HTTPException(status_code=404, detail="Player not found")
    
    players_db[wallet_address]["current_city"] = city
    
    return {
        "success": True,
        "current_city": city
    }

# Transaction endpoints
@app.post("/api/transactions")
async def create_transaction(transaction: Transaction):
    """Record a new transaction"""
    transactions_db.append(transaction.dict())
    
    return {
        "success": True,
        "transaction": transaction
    }

@app.get("/api/transactions/{wallet_address}")
async def get_transactions(wallet_address: str, limit: int = 10):
    """Get transactions for a wallet"""
    user_txs = [
        tx for tx in transactions_db 
        if tx["from_address"] == wallet_address or tx["to_address"] == wallet_address
    ]
    
    return user_txs[:limit]

@app.get("/api/transactions/recent")
async def get_recent_transactions(limit: int = 20):
    """Get recent transactions across all users"""
    return transactions_db[-limit:]

# Property endpoints
@app.post("/api/properties")
async def create_property(property: Property):
    """Register a new property NFT"""
    properties_db.append(property.dict())
    
    return {
        "success": True,
        "property": property
    }

@app.get("/api/properties/{wallet_address}")
async def get_properties(wallet_address: str):
    """Get properties owned by a wallet"""
    user_properties = [
        prop for prop in properties_db 
        if prop["owner_address"] == wallet_address
    ]
    
    return user_properties

@app.get("/api/properties/city/{city}")
async def get_city_properties(city: str, zone_type: Optional[str] = None):
    """Get properties in a specific city"""
    city_properties = [
        prop for prop in properties_db 
        if prop["city"] == city
    ]
    
    if zone_type:
        city_properties = [
            prop for prop in city_properties 
            if prop["zone_type"] == zone_type
        ]
    
    return city_properties

# Economy endpoints
@app.get("/api/economy/token-price")
async def get_token_price():
    """Get current $OMNI token price"""
    # TODO: Calculate from Dominion Economy algorithm
    return {
        "price": 0.015,
        "change_24h": 2.3,
        "timestamp": datetime.now().isoformat()
    }

@app.get("/api/economy/stats")
async def get_economy_stats():
    """Get economy statistics"""
    total_transactions = len(transactions_db)
    total_players = len(players_db)
    total_properties = len(properties_db)
    
    total_volume = sum(tx["amount"] for tx in transactions_db)
    
    return {
        "total_players": total_players,
        "total_transactions": total_transactions,
        "total_properties": total_properties,
        "total_volume": total_volume,
        "circulation_rate": 0.75,  # TODO: Calculate actual rate
        "timestamp": datetime.now().isoformat()
    }

# Quest endpoints
@app.get("/api/quests/available")
async def get_available_quests(city: Optional[str] = None):
    """Get available quests"""
    # TODO: Implement quest generation
    sample_quests = [
        {
            "quest_id": 1001,
            "title": "Welcome to OmniWorld",
            "description": "Complete your first transaction",
            "reward": 10.0,
            "experience_reward": 100,
            "quest_type": "Tutorial",
            "is_active": True
        },
        {
            "quest_id": 1002,
            "title": "Property Owner",
            "description": "Purchase your first property",
            "reward": 50.0,
            "experience_reward": 500,
            "quest_type": "Economic",
            "is_active": True
        }
    ]
    
    return sample_quests

@app.post("/api/quests/{quest_id}/complete")
async def complete_quest(quest_id: int, wallet_address: str):
    """Mark a quest as completed and reward player"""
    if wallet_address not in players_db:
        raise HTTPException(status_code=404, detail="Player not found")
    
    # TODO: Verify quest completion and distribute rewards
    
    return {
        "success": True,
        "message": f"Quest {quest_id} completed",
        "reward_distributed": True
    }

# WebSocket for real-time updates
@app.websocket("/ws/{wallet_address}")
async def websocket_endpoint(websocket: WebSocket, wallet_address: str):
    """WebSocket connection for real-time updates"""
    await websocket.accept()
    active_connections.append(websocket)
    
    try:
        while True:
            # Receive messages from client
            data = await websocket.receive_text()
            
            # Echo back for now (implement real logic later)
            await websocket.send_text(f"Received: {data}")
            
            # Broadcast to all connected clients
            for connection in active_connections:
                if connection != websocket:
                    try:
                        await connection.send_text(f"Player {wallet_address}: {data}")
                    except:
                        pass
    
    except WebSocketDisconnect:
        active_connections.remove(websocket)
        print(f"Player {wallet_address} disconnected")

# AI/GPT Integration endpoints
@app.post("/api/ai/npc-dialogue")
async def generate_npc_dialogue(npc_name: str, player_input: str, context: Dict):
    """Generate NPC dialogue using AI"""
    # TODO: Integrate with GPT API
    
    return {
        "npc_name": npc_name,
        "response": "That's interesting! Tell me more about your adventures in OmniWorld.",
        "sentiment": "positive",
        "suggested_actions": ["Continue conversation", "Ask about quests", "End interaction"]
    }

@app.post("/api/ai/market-analysis")
async def market_analysis(city: str, zone_type: str):
    """Get AI-powered market analysis"""
    # TODO: Implement ML-based market analysis
    
    return {
        "city": city,
        "zone_type": zone_type,
        "trend": "bullish",
        "recommendation": "Good time to invest in this zone",
        "predicted_roi": 18.5,
        "confidence": 0.82
    }

# Admin endpoints
@app.get("/api/admin/stats")
async def get_admin_stats():
    """Get admin statistics"""
    return {
        "total_players": len(players_db),
        "total_transactions": len(transactions_db),
        "total_properties": len(properties_db),
        "active_connections": len(active_connections),
        "uptime": "operational"
    }

if __name__ == "__main__":
    print("Starting OmniWorld Backend API...")
    print("API Documentation: http://localhost:8000/docs")
    uvicorn.run(app, host="0.0.0.0", port=8000)
