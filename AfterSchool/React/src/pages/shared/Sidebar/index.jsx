import React from 'react';
import useStyles from './styles';
import { Box, Typography } from '@material-ui/core';
import { useSearchParams } from 'react-router-dom';

export default function SidebarSection(props) {
  const { questionsGroups } = props;
  const classes = useStyles();
  const [searchParams, setSearchParams] = useSearchParams();
  const sectionName = searchParams.get('section');

  const statusIcons = {
    completed: '✅',
    currentlyEditing: '✏️',
    notStarted: '⏹️',
    notCompleted: '❗'
  };

  const setSectionParam = name => {
    setSearchParams(`section=${encodeURIComponent(name)}`);
  };

  return (
    <Box className={classes.sidebar}>
      <Typography className={classes.sidebarHeader} variant="h6">
        What's Next?
      </Typography>
      <Box className={classes.sectionsListWrapper}>
        {questionsGroups.map((section, index) => (
          <Box
            key={index}
            className={classes.sectionItemWrapper}
            onClick={() => setSectionParam(section.Name)}
          >
            <Typography className={classes.icon}>
              {sectionName === section.Name
                ? statusIcons.currentlyEditing
                : statusIcons[section.Status]}
            </Typography>
            <Typography className={classes.sectionName}>{section.Name}</Typography>
          </Box>
        ))}
      </Box>
      <Typography variant="body2" className={classes.scrollText}>
        Scroll for more
      </Typography>
    </Box>
  );
}
