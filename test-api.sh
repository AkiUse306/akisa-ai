#!/bin/bash

# AKISA-AI API Testing Guide
# Use these cURL commands to test the new endpoints

BASE_URL="http://localhost:5000"
TOKEN=""  # Will be populated after login

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo "=== AKISA-AI API Testing Guide ==="
echo "Make sure the backend is running at $BASE_URL"
echo ""

# 1. REGISTER
echo -e "${BLUE}[1] Register New User${NC}"
REGISTER_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "testpass123",
    "displayName": "Test User"
  }')

echo "Response: $REGISTER_RESPONSE"
TOKEN=$(echo $REGISTER_RESPONSE | jq -r '.accessToken')
USERID=$(echo $REGISTER_RESPONSE | jq -r '.userId')
echo -e "${GREEN}✓ Token: ${TOKEN:0:20}...${NC}"
echo ""

# 2. LOGIN
echo -e "${BLUE}[2] Login${NC}"
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "testpass123"
  }')

echo "Response: $LOGIN_RESPONSE"
TOKEN=$(echo $LOGIN_RESPONSE | jq -r '.accessToken')
REFRESH_TOKEN=$(echo $LOGIN_RESPONSE | jq -r '.refreshToken')
echo -e "${GREEN}✓ New Access Token: ${TOKEN:0:20}...${NC}"
echo -e "${GREEN}✓ Refresh Token: ${REFRESH_TOKEN:0:20}...${NC}"
echo ""

# 3. GET PROFILE
echo -e "${BLUE}[3] Get User Profile${NC}"
curl -s -X GET "$BASE_URL/api/auth/me" \
  -H "Authorization: Bearer $TOKEN" | jq '.'
echo ""

# 4. EXECUTE AGENT - ANALYST
echo -e "${BLUE}[4] Execute Agent: Analyst${NC}"
AGENT_RESPONSE=$(curl -s -X POST "$BASE_URL/api/orchestration/agents" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "input": "What are the top 3 trends in AI right now?",
    "agentType": "analyst"
  }')

echo "Response: $AGENT_RESPONSE" | jq '.'
AGENT_ID=$(echo $AGENT_RESPONSE | jq -r '.agentId')
echo -e "${GREEN}✓ Agent ID: $AGENT_ID${NC}"
echo ""

# 5. EXECUTE AGENT - DEVELOPER
echo -e "${BLUE}[5] Execute Agent: Developer${NC}"
curl -s -X POST "$BASE_URL/api/orchestration/agents" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "input": "Design a REST API for a task management application",
    "agentType": "developer"
  }' | jq '.'
echo ""

# 6. REFRESH TOKEN
echo -e "${BLUE}[6] Refresh Access Token${NC}"
REFRESH_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/refresh" \
  -H "Content-Type: application/json" \
  -d "{
    \"refreshToken\": \"$REFRESH_TOKEN\",
    \"userId\": \"$USERID\"
  }")

echo "Response: $REFRESH_RESPONSE" | jq '.'
NEW_TOKEN=$(echo $REFRESH_RESPONSE | jq -r '.accessToken')
echo -e "${GREEN}✓ New Token: ${NEW_TOKEN:0:20}...${NC}"
TOKEN=$NEW_TOKEN
echo ""

# 7. STREAMING CHAT
echo -e "${BLUE}[7] Stream Chat Response${NC}"
echo "Streaming tokens in real-time:"
curl -s -X POST "$BASE_URL/api/chat/stream" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Explain quantum computing in 2 sentences",
    "model": "gpt-4"
  }' | grep -o '"content":"[^"]*"' | cut -d'"' -f4 | tr -d '\n'
echo ""
echo ""

# 8. TEXT-TO-SPEECH
echo -e "${BLUE}[8] Text-to-Speech${NC}"
echo "Generating audio for: 'Hello from AKISA-AI'"
curl -s -X POST "$BASE_URL/api/voice/text-to-speech" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "text": "Hello from AKISA-AI",
    "voice": "nova"
  }' --output audio_output.mp3

if [ -f audio_output.mp3 ]; then
  SIZE=$(ls -lh audio_output.mp3 | awk '{print $5}')
  echo -e "${GREEN}✓ Audio file created: audio_output.mp3 ($SIZE)${NC}"
else
  echo "No API key configured"
fi
echo ""

# 9. TOOL EXECUTION
echo -e "${BLUE}[9] Execute Tool${NC}"
curl -s -X POST "$BASE_URL/api/orchestration/tools" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "toolId": "sample-plugin",
    "parameters": {
      "text": "This is a test plugin execution"
    }
  }' | jq '.'
echo ""

# 10. MEMORY SEARCH
echo -e "${BLUE}[10] Search Memory${NC}"
curl -s -X GET "$BASE_URL/api/memory/search?query=AI%20trends&limit=3" \
  -H "Authorization: Bearer $TOKEN" | jq '.'
echo ""

# 11. LOGOUT
echo -e "${BLUE}[11] Logout${NC}"
curl -s -X POST "$BASE_URL/api/auth/logout" \
  -H "Authorization: Bearer $TOKEN" | jq '.'
echo -e "${GREEN}✓ Logged out${NC}"
echo ""

echo "=== Testing Complete ==="
