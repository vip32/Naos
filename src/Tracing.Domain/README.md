```
       |-----|-----|-----|-----|-----|-----|-----|-----|-----|-----| =100%

INBREQ [SPAN /api/customers-------------------------]                                        SERVER receives request
INTERN     |---[SPAN validatemodel -----------------]                                        INTERNAL validates the model
INBREQ     |---[SPAN /api/accounts------------------]                                        SERVER receives request for data
DOMREP     :       |---[SPAN getaccount---------]                                            INTERNAL repositpry finds entity
DOMREP     |---[SPAN createentity---------------]                                            INTERNAL repositpry stores entity
DOMEVT     :       |---[SPAN entitycreated------]                                            CONSUMER handles event
MESSAG     :       |---[SPAN customercreated-----------------------]                         CONSUMER handles message
QUEUNG     |---[SPAN emailnewcustomer --------------]                                        CONSUMER handles queue item
```

QUERIES:
- group by operationname where no parent (hit count / avg duration / total duration=sum of all durations)

### Remarks
each span has a parent span, when not it is the root span.
each child span cannot take more time than the root span.
child spans are drawn below the parent span.

a single root span always has 100% width.


**SpanViewModel**
- long:starttime (ms)
- long:endtime   (ms)
- long:duration  (ms)
- int:duration%  (% duration of root span)
- ienumerable<SpanViewModel>:children (server/client/internal/producer/consumer)

- root span = 100% (duration)
