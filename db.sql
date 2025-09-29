create table account_classes(
    id uuid primary key,
    code varchar(10) not null unique,
    name varchar(150) not null unique
);
delete from account_classes;
drop table account_classes;

delete from periods;
drop table periods;
create table periods (
    id uuid primary key,
    date_start date not null,
    date_end date not null,
    unique (date_start, date_end)
);

delete from accounts;
create table accounts(
    id uuid primary key,
    account_number integer not null,
	bank_id uuid references banks(id) on delete set null,
    account_class_id uuid references account_classes(id) on delete set null,

	unique (bank_id, account_number)
);
drop table accounts;

delete from account_balances;
drop table account_balances;
create table account_balances(
    id uuid primary key,
    account_id uuid references accounts(id) on delete set null,
    period_id uuid references periods(id) on delete set null,
	uploaded_file_id uuid not null references uploaded_files(id) on delete set null,
	
    inp_balance_active numeric(18,2) not null,
    inp_balance_passive numeric(18,2) not null,
    turnover_debit numeric(18,2) not null,
    turniver_credit numeric(18,2) not null,
    outp_balance_active numeric(18,2) not null,
    outp_balance_passive numeric(18,2) not null,
	
    unique(account_id, period_id)
);

delete from banks;
create table banks(
	id uuid primary key,
	name varchar(50)
);


delete from uploaded_files;
create table uploaded_files (
    id uuid primary key,
    file_name varchar(255) not null,
    upload_date timestamp not null default now()
);


select * from accounts;

alter table accounts
add constraint accounts_bank_acc_uniq unique (bank_id, account_number);
