create table users
(
    id                      int primary key auto_increment,
    password                varchar(500) not null,
    email                   varchar(500) not null,
    role                    varchar(500) not null,
    remark                  varchar(500),
    is_admin                boolean   default false,
    default_access_token_id int,
    create_time             timestamp default current_timestamp,
    create_user_id          int,
    update_time             timestamp default current_timestamp,
    update_user_id          int,
    delete_time             timestamp default null,
    delete_user_id          int
);

create table access_tokens
(
    id             int primary key auto_increment,
    access_token   varchar(500) not null,
    refresh_token  varchar(500) not null,
    email          varchar(500),
    password       varchar(500),
    remark         varchar(500),
    expire_time    timestamp default null,
    create_time    timestamp default current_timestamp,
    create_user_id int,
    update_time    timestamp default current_timestamp,
    update_user_id int,
    delete_time    timestamp default null,
    delete_user_id int
);

create table conversations
(
    id              int primary key auto_increment,
    conversation_id varchar(500) not null,
    access_token_id int          not null,
    remark          varchar(500) not null,
    create_time     timestamp default current_timestamp,
    create_user_id  int,
    update_time     timestamp default current_timestamp,
    update_user_id  int,
    delete_time     timestamp default null,
    delete_user_id  int
);