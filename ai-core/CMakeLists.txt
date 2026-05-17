cmake_minimum_required(VERSION 3.22)
project(akisa_ai_core LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

find_package(Threads REQUIRED)

add_executable(akisa_core src/main.cpp)

target_include_directories(akisa_core PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/src)

target_link_libraries(akisa_core PRIVATE Threads::Threads)
