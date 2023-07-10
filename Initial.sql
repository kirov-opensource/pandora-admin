create table access_tokens
(
    id             int auto_increment                  primary key,
    access_token   varchar(2000)                       not null,
    refresh_token  varchar(2000)                       not null,
    email          varchar(500)                        null,
    password       varchar(500)                        null,
    remark         varchar(500)                        null,
    expire_time    timestamp                           null,
    create_time    timestamp default CURRENT_TIMESTAMP null,
    create_user_id int                                 null,
    update_time    timestamp default CURRENT_TIMESTAMP null,
    update_user_id int                                 null,
    delete_time    timestamp                           null,
    delete_user_id int                                 null
);

create table conversations
(
    id              int auto_increment                  primary key,
    conversation_id varchar(500)                        not null,
    access_token_id int                                 not null,
    remark          varchar(500)                        null,
    create_time     timestamp default CURRENT_TIMESTAMP null,
    create_user_id  int                                 null,
    update_time     timestamp default CURRENT_TIMESTAMP null,
    update_user_id  int                                 null,
    delete_time     timestamp                           null,
    delete_user_id  int                                 null
);

create table users
(
    id                      int auto_increment                   primary key,
    password                varchar(500)                         not null,
    email                   varchar(500)                         not null,
    role                    varchar(500)                         not null,
    remark                  varchar(500)                         null,
    is_admin                tinyint(1) default 0                 null,
    default_access_token_id int                                  null,
    create_time             timestamp  default CURRENT_TIMESTAMP null,
    create_user_id          int                                  null,
    update_time             timestamp  default CURRENT_TIMESTAMP null,
    update_user_id          int                                  null,
    delete_time             timestamp                            null,
    delete_user_id          int                                  null,
    user_token              varchar(500)                         null
);


INSERT INTO users (password, email, role, remark, is_admin, default_access_token_id, create_time, create_user_id, update_time, update_user_id, delete_time, delete_user_id, user_token) VALUES ('$2a$12$gnaF7njraeVm4iZqybkrLeGyGVK2Gr1zk3hPuLAP05oQhrqItjSU2', 'admin@kirovopensource.com', 'level1', null, 0, 1, '2023-06-25 17:57:25', 0, '2023-06-25 17:57:30', null, null, null, '');

INSERT INTO access_tokens (access_token, refresh_token, email, password, remark, expire_time, create_time, create_user_id, update_time, update_user_id, delete_time, delete_user_id) VALUES ('ACCESS-TOKEN', '1', null, null, null, null, '2023-06-26 22:54:31', null, '2023-06-26 22:54:31', null, null, null);
