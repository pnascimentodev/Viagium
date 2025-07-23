# Viagium

Sistema completo para gestão de viagens, pacotes, reservas, usuários, afiliados, hotéis e pagamentos, desenvolvido em .NET Core.

## Sobre o Projeto

O Viagium é uma API robusta que permite o gerenciamento de todo o ciclo de uma viagem, desde o cadastro de usuários, afiliados e hotéis, até a reserva de pacotes, quartos, pagamentos e avaliações. O sistema foi projetado para ser modular, seguro e escalável, utilizando arquitetura MVC, Entity Framework Core para persistência, AutoMapper para mapeamento de entidades e autenticação JWT.

## Principais Funcionalidades

- Cadastro, autenticação e gerenciamento de usuários, administradores e afiliados
- CRUD de pacotes de viagem, hotéis, quartos, tipos e amenidades
- Reservas de pacotes e quartos
- Processamento de pagamentos
- Sistema de avaliações (reviews)
- Histórico de alterações dos pacotes
- Controle de permissões por roles
- Testes unitários para controllers e serviços

## Estrutura do Projeto

- **Controllers/**: Gerenciam as rotas e requisições HTTP
- **Models/**: Entidades do domínio
- **EntitiesDTO/**: Data Transfer Objects para comunicação entre camadas
- **Repository/**: Repositórios para acesso ao banco de dados
- **Services/**: Lógica de negócio e validações
- **Configurations/**: Configurações globais (ex: JWT)
- **Data/**: Contexto do Entity Framework Core
- **Migrations/**: Migrações do banco de dados
- **ProfileAutoMapper/**: Perfis de mapeamento entre entidades e DTOs
- **Properties/**: Configurações de inicialização
- **Viagium.tests/** e **ViagiumTests/**: Testes unitários

## Como Executar

1. Configure o banco de dados em `appsettings.json`
2. Execute as migrações do Entity Framework Core
3. Inicie o projeto via Rider ou Visual Studio
4. Teste as rotas da API com Postman ou similar

## Tecnologias Utilizadas

- .NET Core
- Entity Framework Core
- AutoMapper
- JWT Authentication

## Entidades do Domínio

O Viagium possui diversas entidades que representam os principais conceitos do sistema. Abaixo estão as principais models e suas funções:

- **User**: Usuários do sistema, incluindo dados pessoais, autenticação e permissões.
- **Admin**: Administradores do sistema, com permissões elevadas.
- **Affiliate**: Afiliados parceiros para venda de pacotes.
- **Address**: Endereços vinculados a usuários, hotéis e afiliados.
- **Hotel**: Hotéis disponíveis nos pacotes de viagem.
- **HotelTypeAmenity**: Relação entre tipos de hotéis e amenidades.
- **Amenity**: Amenidades oferecidas pelos hotéis/quartos.
- **Room**: Quartos disponíveis nos hotéis.
- **RoomType**: Tipos de quartos (ex: suíte, standard).
- **RoomTypeAmenity**: Relação entre tipos de quartos e amenidades.
- **Reservation**: Reservas realizadas pelos usuários.
- **ReservationRoom**: Relação entre reservas e quartos reservados.
- **Traveler**: Viajantes associados a uma reserva.
- **Payment**: Pagamentos realizados para reservas.
- **Review**: Avaliações de pacotes, hotéis ou reservas.
- **Role**: Perfis de acesso e permissões dos usuários.
- **TravelPackage**: Pacotes de viagem disponíveis.
- **TravelPackageHistory**: Histórico de alterações dos pacotes.

## Entidades:
```mermaid
---
config:
  theme: neo-dark
  look: neo
---
erDiagram
    USERS {
        integer id
        varchar firstName
        varchar lastName
        varchar email
        varchar password
        varchar phone
        varchar document_number
        date birthdate
        varchar role
        boolean is_active
        timestamp created_at
        timestamp updated_at
        timestamp deleted_at
    }
    ADMINS {
        integer id
        integer user_id
        timestamp created_at
        timestamp updated_at
    }
    AFFILIATES {
        integer id
        integer user_id
        integer address_id
        timestamp created_at
        timestamp updated_at
    }
    ADDRESSES {
        integer id
        varchar street
        varchar city
        varchar state
        varchar country
        varchar zip_code
        timestamp created_at
        timestamp updated_at
    }
    ROLES {
        integer id
        varchar name
        text description
    }
    HOTELS {
        integer id
        varchar name
        integer address_id
        timestamp created_at
        timestamp updated_at
    }
    HOTEL_TYPE_AMENITIES {
        integer id
        integer hotel_id
        integer amenity_id
    }
    AMENITIES {
        integer id
        varchar name
        text description
    }
    ROOMS {
        integer id
        integer hotel_id
        integer room_type_id
        integer max_guests
        decimal price
        timestamp created_at
        timestamp updated_at
    }
    ROOM_TYPES {
        integer id
        varchar name
        text description
    }
    ROOM_TYPE_AMENITIES {
        integer id
        integer room_type_id
        integer amenity_id
    }
    RESERVATIONS {
        integer id
        integer user_id
        integer travel_package_id
        date start_date
        decimal total_price
        varchar status
        timestamp created_at
    }
    RESERVATION_ROOMS {
        integer id
        integer reservation_id
        integer room_id
    }
    TRAVELERS {
        integer id
        integer reservation_id
        varchar firstName
        varchar lastName
        varchar document_number
        date birthdate
    }
    PAYMENTS {
        integer id
        integer reservation_id
        varchar payment_type
        varchar card_last_digits
        varchar status
        decimal amount
        timestamp paid_at
    }
    REVIEWS {
        integer id
        integer reservation_id
        integer rating
        text description
        timestamp created_at
    }
    TRAVEL_PACKAGES {
        integer id
        varchar title
        text description
        varchar destination
        varchar img_url
        integer duration
        integer max_people
        decimal price
        integer created_by
        timestamp created_at
        timestamp updated_at
    }
    TRAVEL_PACKAGE_HISTORY {
        integer id
        integer reservation_id
        varchar title
        text description
        varchar destination
        integer duration
        decimal price
        timestamp created_at
    }
    %% Relacionamentos
    USERS ||--o{ ADMINS : "id"
    USERS ||--o{ AFFILIATES : "id"
    USERS ||--o{ RESERVATIONS : "user_id"
    USERS ||--o{ TRAVEL_PACKAGES : "created_by"
    USERS ||--o{ ROLES : "role"
    AFFILIATES ||--o{ ADDRESSES : "address_id"
    HOTELS ||--o{ ADDRESSES : "address_id"
    HOTELS ||--o{ ROOMS : "hotel_id"
    HOTELS ||--o{ HOTEL_TYPE_AMENITIES : "hotel_id"
    HOTEL_TYPE_AMENITIES ||--o{ AMENITIES : "amenity_id"
    ROOMS ||--o{ ROOM_TYPES : "room_type_id"
    ROOM_TYPES ||--o{ ROOM_TYPE_AMENITIES : "room_type_id"
    ROOM_TYPE_AMENITIES ||--o{ AMENITIES : "amenity_id"
    RESERVATIONS ||--o{ RESERVATION_ROOMS : "reservation_id"
    RESERVATIONS ||--o{ TRAVELERS : "reservation_id"
    RESERVATIONS ||--o{ PAYMENTS : "reservation_id"
    RESERVATIONS ||--o{ REVIEWS : "reservation_id"
    RESERVATIONS ||--o{ TRAVEL_PACKAGE_HISTORY : "reservation_id"
    RESERVATIONS ||--o{ TRAVEL_PACKAGES : "travel_package_id"
    TRAVEL_PACKAGES ||--o{ HOTELS : "id"
```
 
