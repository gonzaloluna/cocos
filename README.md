# Cocos Trading API

Este es el proyecto de la API de trading que maneja órdenes de compra y venta, validación de fondos y cálculos de portafolio. A continuación se detallan los pasos para compilar, ejecutar y probar el proyecto.

## Requisitos

- [.NET SDK](https://dotnet.microsoft.com/download) (versión 9 o superior)
- [Git](https://git-scm.com/)

## Pasos para compilar el proyecto

1. **Clonar el repositorio** :

git clone https://github.com/gonzaloluna/cocos.git 
cd CocosTradingAPI

2. **Restaurar las dependencias y compilar el proyecto**:

dotnet restore
dotnet build


## Pasos para correr el proyecto

1. **Ejecutar la API**:

Después de compilar el proyecto, se puede ejecutar la API con el siguiente comando:


dotnet run --project CocosTradingAPI.WebAPI

2. **Correr los tests**:

Para ejecutar todos los tests definidos en el proyecto, usa el siguiente comando:

dotnet test

## Documentación

Colección de Postman: La colección de Postman para interactuar con la API se encuentra en el archivo Cocos Web API.postman_collection.json, ubicado en el root del proyecto. Se puede importar en Postman para realizar pruebas de los diferentes endpoints de la API.

Swagger UI: La API también expone su documentación interactiva a través de Swagger. Se puede acceder a ella navegando a la siguiente URL cuando la API esté corriendo:

http://localhost:5185/swagger

Aquí se puede ver y probar todos los endpoints de la API de manera fácil y visual.

## Estructura del Proyecto

- **CocosTradingAPI.Application**: Contiene la lógica de negocio y las estrategias de procesamiento de órdenes.
- **CocosTradingAPI.Domain**: Define los modelos de dominio y las enumeraciones utilizadas en la lógica de la API.
- **CocosTradingAPI.Infrastructure**: Implementación de acceso a datos y servicios externos (si hubiera).
- **CocosTradingAPI.Tests**: Contiene los tests unitarios de la API y las estrategias de ordenes.
- **CocosTradingAPI.WebAPI**: El proyecto web que expone la API a través de HTTP.


