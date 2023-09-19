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

### Authorization?

Don't confuse _authentication_ and _authorization_. The former is _who_ you (potentially)
are, while the latter is _what_ you can do.

Partial authentication can happen inside controllers. Same as validation, this is _"soft
authentication"_; we just make sure that the request is somehow authenticated.

In the handler, however, the _"hard authentication and authorization"_ is done; we want to
make sure that the request belongs to an authenticated entity (user) that _still exists_,
and that the entity (user) is allowed to do what it tries to do.

The thing is, you can have been authenticated at a point in time where you were allowed to
be authenticated. But during the last hour or so, someone blocked your account. Therefore,
on the controller level you are still _authenticated_, but the _handler_ should take care
of what you are allowed to do no matter what.

## API overview

### Users:
- [x] Create a User: `POST /users`
- [x] Log in a User `POST /login`
- [x] Retrieve a User: `GET /users/{user_id}`
- [ ] Update a User: `PUT /users/{user_id}`
- [ ] Delete a User: `DELETE /users/{user_id}`

### Projects:
- [x] Create a Project: `POST /projects`
- [x] Retrieve all Projects for a User: `GET /users/{user_id}/projects`
- [x] Retrieve a Project: `GET /projects/{project_id}`
- [x] Update a Project: `PUT /projects/{project_id}`
- [ ] Delete a Project: `DELETE /projects/{project_id}`

### Boards:
- [x] Create a Board: `POST /projects/{project_id}/boards`
- [x] Retrieve all Boards for a Project: `GET /projects/{project_id}/boards`
- [x] Retrieve a Board: `GET /boards/{board_id}`
- [x] Update a Board: `PUT /boards/{board_id}`
- [ ] Delete a Board: `DELETE /boards/{board_id}`

### Lanes:
- [ ] Create a Lane: `POST /lanes`
- [ ] Retrieve all Lanes for a Board: `GET /boards/{board_id}/lanes`
- [ ] Retrieve a Lane: `GET /lanes/{lane_id}`
- [ ] Update a Lane: `PUT /lanes/{lane_id}`
- [ ] Delete a Lane: `DELETE /lanes/{lane_id}`

### Issues:
- [ ] Create an Issue: `POST /issues`
- [ ] Retrieve all Issues for a Lane: `GET /lanes/{lane_id}/issues`
- [ ] Retrieve an Issue: `GET /issues/{issue_id}`
- [ ] Update an Issue: `PUT /issues/{issue_id}`
- [ ] Delete an Issue: `DELETE /issues/{issue_id}`

## TODO

* Lots

## Author

Tore Aursand <toreau@gmail.com>
