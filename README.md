# Torello

## What is this?

This is my little test bed for best (and/or worst) practices. For a relatively simple
CRUD API, the design is absolutely overkill. 😊 Entities are treated as domains, but
it cold easily be templated, and should be swift to extend and refactor.

## Reasoning

### GUIDs for IDs?

It's not a problem per se to have the database use it's auto increment magic to provide
a unique identity for each entity, but GUIDs can have a few advantages:

1. For distributed systems, you might have to create a unique identity _before_
the database has the opportunity to do so. One example might be that you want to send
a newly created entity to a worker queue for some kind of processing before it's
inserted into the database.
2. They are impossible to guess. But this is security by obscurity, so doesn't really
count.

The biggest disadvantage is that GUIDs are 16 bytes, while a "traditional" identity is
4 or 8 bytes (`uint32` and `uint64`, respectively).

### Empty API project?

To be fair, it's just _almost_ empty; it contains a `HealthController` that an
orchestrator (or similar) can use to to figure out if the application is responding.

The rest of the API is inside the Application domain, because an API _is_ an
application. That's why the Presentation domain is also missing, because it's been
slimmed down to the Application domain instead, because this being an API and all.

## TODO

* Lots

## Author

Tore Aursand <toreau@gmail.com>
