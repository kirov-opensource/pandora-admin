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

