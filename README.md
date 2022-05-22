# Бот для отправки событий о релизах и деплое в  azure dev ops pipelines в Telegram

## Возможности:

Информация о релизе: Состояние билда, проект, репозиторий, ветка, инициатор, коммит, время сборки и прикрепленные задачи

![image](https://user-images.githubusercontent.com/31418624/169696952-2da9c99f-1314-4269-bb7b-d79fcef2d6ea.png)

Информация о несобранных билдов с ссылками на логи

![image](https://user-images.githubusercontent.com/31418624/169698294-0b6547f5-a0b6-4b83-83b4-18c3939cd354.png)


## Как использовать?

Необходимо в ``appsettings.json`` или ``appsettings.X.json`` указать необходимнные ключи и данные:

``

  "AzureOrganization": "", //Имя организации в azure dev ops
  "AzureToken": "", //Токен пользователя с необходимыми правами доступа
  "TelegramToken": "", //Токен телеграм бота
  "TelegramChatIds": [ ], //id чатов куда необходимо сообщать о релизах
  "IdleTime": 60, //Время между запросами о новых релизах
  "IdleInProgressTime": 5 //Время между запросами о новых статусах релиза

``

## Получить токен azure dev ops
![image](https://user-images.githubusercontent.com/31418624/169698514-166cbb64-41e4-4ace-80a4-e02089af5775.png)


Чтобы узнать id текущего чата, можно написать комманду ``/chat``

![image](https://user-images.githubusercontent.com/31418624/169698566-065a2c16-cdcd-458d-886d-22809471bbd4.png)
