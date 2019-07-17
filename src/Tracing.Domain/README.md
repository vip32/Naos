### Timeline view
```
       |-----|-----|-----|-----|-----|-----|-----|-----|-----|-----| =100%

INBREQ [SPAN /api/customers-------------------------]                                        SERVER receives request
......     |---[SPAN validatemodel -----------------]                                        INTERNAL validates the model
INBREQ     |---[SPAN /api/accounts------------------]                                        SERVER receives request for data
DOMREP     :       |---[SPAN getaccount---------]                                            INTERNAL repository finds entity
DOMREP     |---[SPAN createentity---------------]                                            INTERNAL repository stores entity
DOMEVT     :       |---[SPAN entitycreated------]                                            CONSUMER handles event
MESSAG     :       |---[SPAN customercreated----]                                            PRODUCER publishes message
MESSAG     :               |---[SPAN customercreatedhandler--------]                         CONSUMER handles message, takes longer
QUEUNG     |---[SPAN emailnewcustomer ----------]                                            CONSUMER handles queue item
```

### Sequence view
```
LOGKEY OPERATIONANAME (^=logs)                                 TOOK  KIND     DESCRIPTION
----------------------------------------------------------------------------------------------------------
INBREQ [SPAN /api/customers]^                                  300ms SERVER   receives request
......     |---[SPAN validatemodel]^                           100ms INTERNAL validates the model
INBREQ     |---[SPAN /api/accounts]^                            10ms SERVER   receives request for data
DOMREP     |       `---[SPAN getaccount]^                        9ms INTERNAL repository finds entity
DOMREP     |---[SPAN createentity]^                              9ms INTERNAL repository stores entity
DOMEVT     |       |---[SPAN entitycreated]^                     3ms CONSUMER handles event
MESSAG     |       `---[SPAN customercreated]^                   2ms PRODUCER publishes message
MESSAG     |               `---[SPAN customercreatedhandler]^   21ms CONSUMER handles message, takes longer
QUEUNG     `---[SPAN emailnewcustomer]^                         11ms CONSUMER handles queue item
```

SPANNAME >> PRODUCT.CAPABILITY::OPERATIONNAME

QUERIES:
- group by operationname where span=root (hit count / avg duration / total duration=sum of all durations)

### Remarks
each span has a parent span, when not it is the root span.
each child span cannot take more time than the root span.
child spans are drawn below the parent span.



the root span starttime till endtime represents 100% width. 
if any child span takes longer (max) than the root span that endtime will be used instead for 100% width.

diagram is not scaled for starttimes, each indent means a CHILD and does not represent the actual starttime.
diagram endtimes are somehow scaled and represented by the width (%) of the bar


**SpanViewModel**
- long:starttime (ms)
- long:endtime   (ms)
- long:duration  (ms)
- int:duration%  (% duration of root span)
- ienumerable<SpanViewModel>:children (server/client/internal/producer/consumer)

- root span = 100% (duration)
