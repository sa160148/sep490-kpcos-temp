--drop database kpcos;
--create database kpcos;
-- database

-- Start transaction to ensure all-or-nothing execution
BEGIN;

-- Set error handling to abort transaction on any error
\set ON_ERROR_ROLLBACK on
\set ON_ERROR_STOP on

-- Set timezone to Bangkok (UTC+7)
SET timezone = 'Asia/Bangkok';

-- Trigger function to update updated_at column when row is updated
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW() AT TIME ZONE 'Asia/Bangkok';
    RETURN NEW;
END;
$$ LANGUAGE 'plpgsql';

-- create construction template table
CREATE table construction_template(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    status VARCHAR(255)
);

-- Trigger to update updated_at column when row is updated in construction_template table
CREATE TRIGGER update_construction_template_updated_at
BEFORE UPDATE ON construction_template
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Indexes

-- create construction template item table
CREATE table construction_template_item(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    category VARCHAR(255),
    description TEXT,
    idParent UUID,
    idTemplate UUID NOT NULL,
    duration INT NOT NULL,
    status VARCHAR(255),
    FOREIGN KEY (idParent) REFERENCES construction_template_item(id),
    FOREIGN KEY (idTemplate) REFERENCES construction_template(id)
);

-- Trigger to update updated_at column when row is updated in construction_template_item table
CREATE TRIGGER update_construction_template_item_updated_at
BEFORE UPDATE ON construction_template_item
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Indexes

-- create construction template task table
CREATE table construction_template_task(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    idTemplateItem UUID NOT NULL,
    status VARCHAR(255),
    FOREIGN KEY (idTemplateItem) REFERENCES construction_template_item(id)
);

-- Trigger to update updated_at column when row is updated in construction_template_task table
CREATE TRIGGER update_construction_template_task_updated_at
BEFORE UPDATE ON construction_template_task
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Indexes

-- Create users table
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    email VARCHAR(255) NOT NULL,
    password VARCHAR(255) NOT NULL,
    full_name VARCHAR(255) NOT NULL,
    phone VARCHAR(255) NOT NULL,
    avatar VARCHAR(255) DEFAULT 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTaotZTcu1CLMGOJMDl-f_LYBECs7tqwhgpXA&s',
    status VARCHAR(255)
);

-- Trigger to update updated_at column when row is updated in users table
CREATE TRIGGER update_users_updated_at
BEFORE UPDATE ON users
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Create customer table
CREATE table customer(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    point INT DEFAULT 0,
    address VARCHAR(255) NOT NULL,
    dob DATE NOT NULL DEFAULT '2000-01-01',
    gender VARCHAR(255) NOT NULL,
    user_id UUID NOT NULL,
    FOREIGN KEY (user_id) REFERENCES users(id)
);

-- Trigger to update updated_at column when row is updated in customer table
CREATE TRIGGER update_customer_updated_at
BEFORE UPDATE ON customer
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Indexes

-- Create staff table
CREATE table staff(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    position VARCHAR(255) NOT NULL,
    user_id UUID NOT NULL,
    FOREIGN KEY (user_id) REFERENCES users(id)
);

-- Trigger to update updated_at column when row is updated in staff table
CREATE TRIGGER update_staff_updated_at
BEFORE UPDATE ON staff
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Indexes

-- Create package item table
CREATE table package_item(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL
);

-- Trigger to update updated_at column when row is updated in package_item table
CREATE TRIGGER update_package_item_updated_at
BEFORE UPDATE ON package_item
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- indexs

-- Create package table
CREATE table package(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    price int NOT NULL,
    rate int not null,
    status VARCHAR(255)
);

-- Trigger to update updated_at column when row is updated in package table
CREATE TRIGGER update_package_updated_at
BEFORE UPDATE ON package
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Indexes

-- Create package detail table
CREATE table package_detail(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    quantity INT,
    description TEXT,
    package_id UUID NOT NULL,
    package_item_id UUID NOT NULL,
    FOREIGN KEY (package_id) REFERENCES package(id),
    FOREIGN KEY (package_item_id) REFERENCES package_item(id)
);

-- Trigger to update updated_at column when row is updated in package_detail table
CREATE TRIGGER update_package_detail_updated_at
BEFORE UPDATE ON package_detail
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Indexes

-- Create project table
CREATE table project(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    customer_name VARCHAR(255) NOT NULL,
    address VARCHAR(255) NOT NULL,
    phone VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    area float NOT NULL,
    depth float NOT NULL,
    package_id UUID NOT NULL,
    note TEXT,
    status VARCHAR(255),
    customer_id UUID NOT NULL,
    templateDesignId UUID,
    FOREIGN KEY (package_id) REFERENCES package(id),
    FOREIGN KEY (customer_id) REFERENCES customer(id)
);

-- Trigger to update updated_at column when row is updated in project table
CREATE TRIGGER update_project_updated_at
BEFORE UPDATE ON project
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Create project staff table
CREATE table project_staff(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    project_id UUID NOT NULL,
    staff_id UUID NOT NULL,
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (staff_id) REFERENCES staff(id)
);

-- Trigger to update updated_at column when row is updated in project_staff table
CREATE TRIGGER update_project_staff_updated_at
BEFORE UPDATE ON project_staff
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Create equipment table
CREATE table equipment(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    description TEXT
);

-- Trigger to update updated_at column when row is updated in equipment table
CREATE TRIGGER update_equipment_updated_at
BEFORE UPDATE ON equipment
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Create service table
CREATE table service(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    type VARCHAR(255) NOT NULL,
    unit VARCHAR(255) NOT NULL,
    price int NOT NULL,
    status VARCHAR(255)
);

-- Trigger to update updated_at column when row is updated in service table
CREATE TRIGGER update_service_updated_at
BEFORE UPDATE ON service
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Create promotion table
CREATE table promotion(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    code TEXT NOT NULL,
    discount INT NOT NULL,
    startTime TIMESTAMPTZ NOT NULL,
    expTime TIMESTAMPTZ NOT NULL,
    status VARCHAR(255)
);

-- Trigger to update updated_at column when row is updated in promotion table
CREATE TRIGGER update_promotion_updated_at
BEFORE UPDATE ON promotion
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Create quotation table
CREATE table quotation(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    project_id UUID NOT NULL,
    version INT NOT NULL,
    total_price INT NOT NULL,
    reason TEXT ,
    status VARCHAR(255),
    promotion_id UUID,
    idTemplate uuid not null,
    FOREIGN KEY (promotion_id) REFERENCES promotion(id),
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (idTemplate) REFERENCES construction_template(id)
);

-- Trigger to update updated_at column when row is updated in quotation table
CREATE TRIGGER update_quotation_updated_at
BEFORE UPDATE ON quotation
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Create quotation detail table
CREATE table quotation_detail(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    quantity INT NOT NULL,
    price INT NOT NULL,
    category VARCHAR(255) NOT NULL,
    note TEXT,
    quotation_id UUID NOT NULL,
    service_id UUID NOT NULL,
    FOREIGN KEY (quotation_id) REFERENCES quotation(id),
    FOREIGN KEY (service_id) REFERENCES service(id)
);

-- Trigger to update updated_at column when row is updated in quotation_detail table
CREATE TRIGGER update_quotation_detail_updated_at
BEFORE UPDATE ON quotation_detail
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Create quotation equipment table
CREATE table quotation_equipment(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    quantity INT NOT NULL,
    price INT NOT NULL,
    note TEXT,
    category VARCHAR(255) NOT NULL,
    quotation_id UUID NOT NULL,
    equipment_id UUID NOT NULL,
    FOREIGN KEY (quotation_id) REFERENCES quotation(id),
    FOREIGN KEY (equipment_id) REFERENCES equipment(id)
);

-- Trigger to update updated_at column when row is updated in quotation_equipment table
CREATE TRIGGER update_quotation_equipment_updated_at
BEFORE UPDATE ON quotation_equipment
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Create design table
CREATE table design(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    version INT NOT NULL,
    reason TEXT,
    status VARCHAR(255),
    is_public BOOLEAN DEFAULT FALSE,
    project_id UUID not null,
    staff_id UUID not null,
    FOREIGN KEY (staff_id) REFERENCES staff(id),
    FOREIGN KEY (project_id) REFERENCES project(id),
    type VARCHAR not null
);

-- Trigger to update updated_at column when row is updated in design table
CREATE TRIGGER update_design_updated_at
BEFORE UPDATE ON design
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Create design image
CREATE table design_image(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    image_url VARCHAR(255) NOT NULL,
    design_id UUID NOT NULL,
    FOREIGN KEY (design_id) REFERENCES design(id)
);

-- Trigger to update updated_at column when row is updated in design_image table
CREATE TRIGGER update_design_image_updated_at
BEFORE UPDATE ON design_image
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- create doc_tyoe table
CREATE TABLE doc_type(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL
);

-- create docs table
CREATE table docs(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    url VARCHAR(255) NOT NULL,
    doc_type_id UUID NOT NULL,
    status VARCHAR(255),
    project_id UUID NOT NULL,
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (doc_type_id) REFERENCES doc_type(id)
);

-- Trigger to update updated_at column when row is updated in docs table
CREATE TRIGGER update_docs_updated_at
BEFORE UPDATE ON docs
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

CREATE table doc_type(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255)
);

-- create contract table
CREATE table contract(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    customer_name VARCHAR(255) NOT NULL,
    contract_value INT NOT NULL,
    url VARCHAR(255) NOT NULL,
    note TEXT,
    quotation_id UUID NOT NULL,
    project_id UUID NOT NULL,
    FOREIGN KEY (project_id) REFERENCES project(id),
    status VARCHAR(255)
);

-- Trigger to update updated_at column when row is updated in contract table
CREATE TRIGGER update_contract_updated_at
BEFORE UPDATE ON contract
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- create payment batch table
CREATE table payment_batch(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    total_value DECIMAL NOT NULL,
    is_paid BOOLEAN DEFAULT FALSE,
    contract_id UUID NOT NULL,
    construction_item_id UUID,
    payment_phase VARCHAR(255),
    payment_at TIMESTAMP,
    percents INT,
    FOREIGN KEY (contract_id) REFERENCES contract(id),
    status VARCHAR(255)
);

-- Trigger to update updated_at column when row is updated in payment_batch table
CREATE TRIGGER update_payment_batch_updated_at
BEFORE UPDATE ON payment_batch
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- create transaction table
CREATE table transaction(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    customer_id UUID NOT NULL,
    type VARCHAR(255) NOT NULL,
    no UUID NOT NULL,
    amount INT NOT NULL,
    note TEXT,
    status VARCHAR(255),
    FOREIGN KEY (customer_id) REFERENCES customer(id)
);

-- Trigger to update updated_at column when row is updated in transaction table
CREATE TRIGGER update_transaction_updated_at
BEFORE UPDATE ON transaction
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- create construction item table
CREATE table construction_item(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    category VARCHAR(255),
    estimate_at DATE NOT NULL,
    actual_at DATE,
    parent_id UUID,
    project_id UUID NOT NULL,
    is_payment BOOLEAN DEFAULT FALSE,
    status VARCHAR(255),
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- Trigger to update updated_at column when row is updated in construction_item table
CREATE TRIGGER update_construction_item_updated_at
BEFORE UPDATE ON construction_item
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- create construction task table
CREATE table construction_task(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    construction_item_id UUID NOT NULL,
    image_url VARCHAR(255),
    reason TEXT,
    staff_id UUID,
    deadline_at TIMESTAMP,
    deadline_actual_at TIMESTAMP,
    status VARCHAR(255),
    FOREIGN KEY (construction_item_id) REFERENCES construction_item(id),
    FOREIGN KEY (staff_id) REFERENCES staff(id)
);

-- Trigger to update updated_at column when row is updated in construction_task table
CREATE TRIGGER update_construction_task_updated_at
BEFORE UPDATE ON construction_task
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- create maintenance package table
CREATE table maintenance_package(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    price INT NOT NULL,
    status VARCHAR(255)
);

-- Trigger to update updated_at column when row is updated in maintenance_package table
CREATE TRIGGER update_maintenance_package_updated_at
BEFORE UPDATE ON maintenance_package
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- create maintenance item table
CREATE table maintenance_item(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    description TEXT
);

-- Trigger to update updated_at column when row is updated in maintenance_item table
CREATE TRIGGER update_maintenance_item_updated_at
BEFORE UPDATE ON maintenance_item
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- create maintenance package item table
CREATE table maintenance_package_item(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    maintenance_package_id UUID NOT NULL,
    maintenance_item_id UUID NOT NULL,
    FOREIGN KEY (maintenance_package_id) REFERENCES maintenance_package(id),
    FOREIGN KEY (maintenance_item_id) REFERENCES maintenance_item(id)
);

-- Trigger to update updated_at column when row is updated in maintenance_package_item table
CREATE TRIGGER update_maintenance_package_item_updated_at
BEFORE UPDATE ON maintenance_package_item
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- create maintenance request table
CREATE table maintenance_request(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    is_paid BOOLEAN DEFAULT FALSE,
    name VARCHAR(255),
    address VARCHAR(255),
    area FLOAT NOT NULL,
    depth FLOAT NOT NULL,
    total_value BIGINT NOT NULL ,
    type VARCHAR(255),
    customer_id UUID NOT NULL,
    maintenance_package_id UUID NOT NULL,
    status VARCHAR(255),
    FOREIGN KEY (customer_id) REFERENCES customer(id),
    FOREIGN KEY (maintenance_package_id) REFERENCES maintenance_package(id)
);

-- Trigger to update updated_at column when row is updated in maintenance_request table
CREATE TRIGGER update_maintenance_request_updated_at
BEFORE UPDATE ON maintenance_request
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- create maintenance request task table
CREATE table maintenance_request_task(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    maintenance_request_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    reason TEXT,
    estimate_at DATE,
    staff_id UUID NOT NULL,
    status VARCHAR(255),
    image_url VARCHAR(255),
    FOREIGN KEY (maintenance_request_id) REFERENCES maintenance_request(id),
    FOREIGN KEY (staff_id) REFERENCES staff(id)
);

-- Trigger to update updated_at column when row is updated in maintenance_request_task table
CREATE TRIGGER update_maintenance_request_task_updated_at
BEFORE UPDATE ON maintenance_request_task
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

CREATE TABLE issue_type (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL
);

CREATE TRIGGER update_issue_type_updated_at
BEFORE UPDATE ON issue_type
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- create table project issue
CREATE TABLE project_issue (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'Asia/Bangkok'),
    is_active BOOLEAN DEFAULT TRUE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    solution TEXT,
    cause TEXT,
    issue_image VARCHAR(255),
    confirm_image VARCHAR(255),
    reason TEXT,
    estimate_at DATE,
    actual_at DATE,
    status VARCHAR(255),
    issue_type_id UUID NOT NULL,
    staff_id UUID,
    construction_item_id UUID NOT NULL,
    FOREIGN KEY (issue_type_id) REFERENCES issue_type(id),
    FOREIGN KEY (staff_id) REFERENCES staff(id),
    FOREIGN KEY (construction_item_id) REFERENCES construction_item(id)
);

-- Create trigger for project_issue updated_at
CREATE TRIGGER update_project_issue_updated_at
BEFORE UPDATE ON project_issue
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- Commit the transaction if everything succeeded
COMMIT;