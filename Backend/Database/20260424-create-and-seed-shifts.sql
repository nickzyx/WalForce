CREATE TABLE IF NOT EXISTS public.shifts (
    shift_id uuid PRIMARY KEY,
    employee_id integer NOT NULL REFERENCES public.employees(employeeid),
    date date NOT NULL,
    timestart time without time zone NOT NULL,
    timeend time without time zone NOT NULL,
    role_label character varying NOT NULL,
    note character varying NULL
);

CREATE INDEX IF NOT EXISTS shifts_employee_date_idx
    ON public.shifts (employee_id, date);

CREATE INDEX IF NOT EXISTS shifts_date_idx
    ON public.shifts (date);

INSERT INTO public.shifts (shift_id, employee_id, date, timestart, timeend, role_label, note) VALUES
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da001', 1, '2026-04-13', '08:00:00', '16:00:00', 'Cashier', 'Opening shift'),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da002', 2, '2026-04-13', '12:00:00', '20:00:00', 'Stock Associate', NULL),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da003', 3, '2026-04-14', '09:00:00', '17:00:00', 'Customer Service', NULL),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da004', 1, '2026-04-15', '10:00:00', '18:00:00', 'Cashier', NULL),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da005', 2, '2026-04-15', '11:00:00', '19:00:00', 'Stock Associate', 'Truck delivery'),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da006', 3, '2026-04-16', '13:00:00', '21:00:00', 'Customer Service', NULL),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da007', 1, '2026-04-17', '08:00:00', '16:00:00', 'Cashier', NULL),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da008', 2, '2026-04-18', '09:00:00', '17:00:00', 'Stock Associate', 'Weekend coverage'),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da009', 3, '2026-04-19', '10:00:00', '18:00:00', 'Customer Service', NULL),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da010', 1, '2026-04-20', '08:00:00', '16:00:00', 'Cashier', 'Opening shift'),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da011', 2, '2026-04-21', '12:00:00', '20:00:00', 'Stock Associate', NULL),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da012', 3, '2026-04-21', '09:00:00', '17:00:00', 'Customer Service', NULL),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da013', 1, '2026-04-22', '10:00:00', '18:00:00', 'Cashier', NULL),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da014', 2, '2026-04-23', '11:00:00', '19:00:00', 'Stock Associate', NULL),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da015', 3, '2026-04-24', '13:00:00', '21:00:00', 'Customer Service', 'Late support'),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da016', 1, '2026-04-25', '09:00:00', '17:00:00', 'Cashier', 'Weekend coverage'),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da017', 2, '2026-04-26', '09:00:00', '17:00:00', 'Stock Associate', NULL),
    ('ce7af8ad-4f06-4e56-9601-b0c80e0da018', 3, '2026-04-26', '10:00:00', '18:00:00', 'Customer Service', NULL)
ON CONFLICT (shift_id) DO UPDATE SET
    employee_id = EXCLUDED.employee_id,
    date = EXCLUDED.date,
    timestart = EXCLUDED.timestart,
    timeend = EXCLUDED.timeend,
    role_label = EXCLUDED.role_label,
    note = EXCLUDED.note;
